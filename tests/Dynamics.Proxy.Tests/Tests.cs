using Assistant.Net.Dynamics.Proxy.Abstractions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Assistant.Net.Dynamics.Proxy.Tests
{
    public class Tests
    {
        //[Test]
        //public void Test1()
        //{
        //    var obj2 = DynamicProxy.Create<IA>()
        //        .Intercept(x => x.Method<IEnumerable<char>>(default!), _ => { })
        //        .Intercept(x => x.Method<IEnumerable<char>>(default!), (_, _) => { })
        //        .Intercept(x => x.Method<IEnumerable<char>>(default!), (_, _, _) => { })
        //        .Intercept(x => x.Property, "5")
        //        .Intercept(x => x.Function(), _ => "6")
        //        .Intercept(x => x.Function(default!), (_, _) => "7")
        //        .Intercept(x => x.Function(default!, default!), (_, _, _) => 8)
        //        .GetObject();

        //    obj2.Method<IEnumerable<char>>(default!);
        //    var property = obj2.Property;
        //    var function1 = obj2.Function();
        //    var function2 = obj2.Function(default!);
        //    var function3 = obj2.Function(default!, default);

        //}

        [Test]
        public void Test2()
        {
            //var watch1 = Stopwatch.StartNew();
            //for (int i = 0; i < 1_000_000; i++)
            //    new A();
            //var iCreateTime = watch1.Elapsed;
            //for (int i = 0; i < 1_000_000; i++)
            //    DynamicProxy.Create<IA>()
            //        .Intercept(x => x.Method(default!), _ => { })
            //        .Intercept(x => x.Method(default!), (_, _) => { })
            //        .Intercept(x => x.Method(default!), (_, _, _) => { })
            //        .Intercept(x => x.Property, "5")
            //        .Intercept(x => x.Function(), _ => "6")
            //        .Intercept(x => x.Function(default!), (_, _) => "7")
            //        .Intercept(x => x.Function(default!, default!), (_, _, _) => 8)
            //        .GetObject();
            //watch1.Stop();
            //var pCreateTime = watch1.Elapsed - iCreateTime;
            //var dif1 = pCreateTime / iCreateTime;

            var a = new A();
            var compilation = CSharpCompilation.Create("assembly-name")
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddProxy<IA>();
            using var memory = new MemoryStream();
            var result = compilation.Emit(memory);
            memory.Seek(0, SeekOrigin.Begin);
            var assembly = Assembly.Load(memory.ToArray());
            var type = assembly.GetType("Assistant.Net.Proxies.IAProxy")!;
            var b = ((Proxy<IA>)Activator.CreateInstance(type, new A())!)
            //var b = DynamicProxy.Create<IA>(a)
                //.Intercept(x => x.Method<string>(default!), _ => { })
                //.Intercept(x => x.Method<string>(default!), (_, _) => { })
                //.Intercept(x => x.Method<string>(default!), (_, _, _) => { })
                //.Intercept(x => x.Property, "5")
                //.Intercept(x => x.Function(), _ => "6")
                //.Intercept(x => x.Function(default!), (_, _) => "7")
                .Intercept(x => x.Function(default!, default!), (_, _, _) => 8)
                .Object;
            var watch2 = Stopwatch.StartNew();
            for (int i = 0; i < 1_000_000; i++)
            {
                //a.Method<string>(default!);
                //var a1 = a.Property;
                //var a2 = a.Function();
                //var a3 = a.Function(default!);
                var a4 = a.Function("1", 2);
            }
            var iCallTime = watch2.Elapsed;
            for (int i = 0; i < 1_000_000; i++)
            {
                //b.Method<string>(default!);
                //var b1 = b.Property;
                //var b2 = b.Function();
                //var b3 = b.Function(default!);
                var b4 = b.Function("1", 2);
            }
            watch2.Stop();
            var pCallTime = watch2.Elapsed - iCallTime;
            var dif2 = pCallTime / iCallTime;

            //var a = new A();
            //var Proxy<T> = DynamicProxy.Create<IA>(a).GetObject();
            //var b = proxy.Property;
            //var c = proxy.Function();
            //var d = proxy.Function(default!);
            //var e = proxy.Function(default!, default);
        }

        [Test]
        public void Test3()
        {
            var watch = Stopwatch.StartNew();

            var factory = new ServiceCollection()
                .AddProxyFactory(o => o.Add<IA>())
                .BuildServiceProvider()
                .GetRequiredService<IProxyFactory>();

            var proxy = factory.Create<IA>(new A())
                .Intercept(x => x.Method<string>(default!), _ => { })
                .Intercept(x => x.Method<string>(default!), (_, _) => { })
                .Intercept(x => x.Method<string>(default!), (_, _, _) => { })
                .Intercept(x => x.Method(), (_, _, _) => { })
                .Intercept(x => x.Property, "5")
                .Intercept(x => x.Function(), _ => "6")
                .Intercept(x => x.Function(default!), (_, _) => "7")
                .Intercept(x => x.Function(default!, default!), (_, _, _) => 8)
                .Object;

            watch.Stop();

            var str = proxy.ToString();
            var b = proxy.Property;
            var c = proxy.Function();
            var d = proxy.Function(default!);
            var e = proxy.Function(default!, default);
            var f = proxy.Method();
            proxy.Method("ss");
        }

        [Test]
        public void Test4()
        {
            var factory = new ServiceCollection()
                .AddProxyFactory()
                .BuildServiceProvider()
                .GetRequiredService<IProxyFactory>();

            var x = KnownProxy.ProxyTypes;

            var proxy = factory.Create<IA>().Object;
            var str = proxy.ToString();
            var b = proxy.Property;
            var c = proxy.Function();
            var d = proxy.Function(default!);
            var e = proxy.Function(default!, default);
            var f = proxy.Method();
        }
    }
    
    public interface IA
    {
        string Property { get; }
        string Property2 { set; }
        string Function();
        string Function(string a);
        int Function(string a, int b);
        void Method<T>(T a) where T : class, IEnumerable<char>;
        Task Method();
        event Action Event;
    }

    public record A : IA
    {
        public string Property => "1";
        public string Property2
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public string Function() => "2";
        public string Function(string a) => "3";
        public int Function(string a, int b) => 4;
        public void Method<T>(T a) where T : class, IEnumerable<char> { }
        public Task Method() => Task.CompletedTask;
        public event Action Event = delegate { };
    }
}