using System;
using System.Collections.Generic;
using System.Reflection;

namespace Assistant.Net.Dynamics.ProxyAnalyzer
{
    /// <summary>
    ///     Proxy abstraction.
    /// </summary>
    public interface IProxy
    {
        Dictionary<MethodInfo, Func<Func<object?[], object?>, object?[], object?>> Interceptors { get; }
    }
}