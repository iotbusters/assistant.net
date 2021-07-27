﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Assistant.Net.Analyzers.Tests.Mocks
{
    public class Test : ITest
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