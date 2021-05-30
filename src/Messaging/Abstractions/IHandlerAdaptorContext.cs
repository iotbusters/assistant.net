namespace Assistant.Net.Messaging.Abstractions
{
    public interface IHandlerAdaptorContext
    {
         void Init(IAbstractCommandHandler handler);
    }
}