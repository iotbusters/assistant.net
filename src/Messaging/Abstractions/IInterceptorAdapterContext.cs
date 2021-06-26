namespace Assistant.Net.Messaging.Abstractions
{
    public interface IInterceptorAdapterContext
    {
         void Init(IAbstractCommandInterceptor interceptor);
    }
}