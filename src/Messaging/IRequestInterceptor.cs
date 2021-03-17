using System;
using System.Threading.Tasks;
using Assistant.Net.Messaging;

namespace Messaging
{
    public interface IRequestInterceptor : IInterceptor
    {
        Task Intercept(IRequest request, Action next);
    }

    public interface IRequestInterceptor<TResponse> : IInterceptor
    {
        Task<TResponse> Intercept(IRequest<TResponse> request, Func<IRequest<TResponse>, Task<TResponse>> next);
    }
    public interface IInterceptor
    {
    }
}