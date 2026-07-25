using System;

namespace Playcenter.MobileCore
{
    /// <summary>Live session scope. Dispose tears the scope down.</summary>
    public interface ISessionScopeHandle : IDisposable
    {
        T Get<T>() where T : class;
        bool TryGet<T>(out T service) where T : class;
    }
}
