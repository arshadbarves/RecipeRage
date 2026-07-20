using System;

namespace Playcenter.SDK
{
    public interface IServiceRegistry
    {
        void AddSingleton<TService>(TService instance) where TService : class;
        void AddSingleton<TService, TImpl>() where TService : class where TImpl : class, TService, new();
        void AddSingleton<TService>(Func<IPlaycenterServices, TService> factory) where TService : class;
        IPlaycenterServices Build();
    }
}
