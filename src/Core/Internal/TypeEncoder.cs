using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Assistant.Net.Abstractions;

namespace Assistant.Net.Internal
{

    /// <summary>
    ///     Default implementation of type encoder
    ///     with short type name based behavior.
    /// </summary>
    internal class TypeEncoder : ITypeEncoder, IDisposable
    {
        private readonly Dictionary<string, Type> knownTypes = new();

        public TypeEncoder()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                AddTypes(assembly);
            AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoad;
        }

        void IDisposable.Dispose() => AppDomain.CurrentDomain.AssemblyLoad -= OnAssemblyLoad;

        public Type? Decode(string encodedType) =>
            knownTypes.TryGetValue(encodedType, out var type)
            ? type
            : null;

        public string Encode(Type type) => type.Name;

        private void AddTypes(Assembly assembly)
        {
            var types = assembly.GetTypes()
                .Where(x => !string.IsNullOrEmpty(x.Namespace));
            foreach (var type in types)
            {
                var encodedType = Encode(type);
                if (!knownTypes.ContainsKey(encodedType))
                    knownTypes.Add(encodedType, type);
            }
        }

        private void OnAssemblyLoad(object? sender, AssemblyLoadEventArgs args) => AddTypes(args.LoadedAssembly);
    }
}