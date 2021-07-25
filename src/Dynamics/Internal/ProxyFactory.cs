using Assistant.Net.Dynamics.Options;
using Assistant.Net.Dynamics.ProxyAnalyzer;
using Assistant.Net.Dynamics.ProxyAnalyzer.Abstractions;
using Assistant.Net.Dynamics.ProxyAnalyzer.Builders;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Assistant.Net.Dynamics.Internal
{
    /// <summary>
    ///     Default dynamic proxy factory implementation based on <see cref="KnownProxy"/> global proxy registry.
    /// </summary>
    internal sealed class ProxyFactory : IProxyFactory
    {
        public ProxyFactory(IOptions<ProxyFactoryOptions> options)
        {
            var proxyTypes = options.Value.ProxyTypes;
            var unknownTypes = proxyTypes.Where(x => !KnownProxy.ProxyTypes.Keys.Contains(x)).ToArray();
            if (!unknownTypes.Any())
                return;

            // todo: implement compiled proxy caching
            //var hash = unknownTypes.Aggregate(0, HashCode.Combine);
            //if (File.Exists(ProxyAssemblyLocation(hash)))
            //{
            //    KnownProxy.RegisterFrom(Assembly.LoadFile(ProxyAssemblyLocation(0)));
            //    return;
            //}

            Compilation compilation = CSharpCompilation.Create(ProxyAssemblyName)
                .WithOptions(new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary,
                    optimizationLevel: OptimizationLevel.Release));

            foreach (var proxyType in unknownTypes)
                compilation = compilation.AddProxy(proxyType);
            
            using var memory = new MemoryStream();
            var result = compilation.Emit(memory);
            if (!result.Success)
                throw new InvalidOperationException($"Compilation failed with {result.Diagnostics.Length} errors.");

            memory.Seek(0, SeekOrigin.Begin);
            var rawAssembly = memory.ToArray();

            // todo: implement compiled proxy caching
            //using (var file = File.OpenWrite(ProxyAssemblyLocation(hash)))
            //    file.Write(rawAssembly, 0, rawAssembly.Length);

            KnownProxy.RegisterFrom(Assembly.Load(rawAssembly));
        }

        Proxy<T> IProxyFactory.Create<T>(T? instance) where T : class
        {
            if (!KnownProxy.ProxyTypes.TryGetValue(typeof(T), out var proxyType))
                throw new InvalidOperationException($"Proxy of instance type '{typeof(T)}' wasn't configured yet.");

            return (Proxy<T>?) Activator.CreateInstance(proxyType, instance)
                   ?? throw new InvalidOperationException($"Proxy '{proxyType}' wasn't created.");
        }

        // todo: implement compiled proxy caching
        //private static string ProxyAssemblyLocation
        //{
        //    get
        //    {
        //        var location = Assembly.GetExecutingAssembly().Location;
        //        var folder = Path.GetDirectoryName(location)!;
        //        return Path.Combine(folder, ProxyAssemblyName);
        //    }
        //}

        private static string ProxyAssemblyName
        {
            get
            {
                var location = Assembly.GetExecutingAssembly().Location;
                var fileName = Path.GetFileNameWithoutExtension(location);
                return fileName + ".proxies.dll";
            }
        }
    }
}