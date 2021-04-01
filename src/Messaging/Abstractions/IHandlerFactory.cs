using System;

namespace Assistant.Net.Messaging.Abstractions
{
    public interface IHandlerFactory
    {
        IAbstractHandler Create(Type commandType);
    }
}