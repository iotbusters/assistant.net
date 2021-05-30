namespace Assistant.Net.Messaging.Abstractions
{
    public interface IInterceptorAdaptorContext
    {
         void Init(IAbstractCommandInterceptor interceptor);
    }
}