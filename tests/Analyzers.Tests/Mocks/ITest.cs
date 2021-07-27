﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Assistant.Net.Analyzers.Tests.Mocks
{
    public interface ITest
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
}