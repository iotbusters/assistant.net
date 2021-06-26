namespace Assistant.Net.Messaging.Abstractions
{
    public interface IHandlerAdapterContext
    {
         void Init(IAbstractCommandHandler handler);
    }
}