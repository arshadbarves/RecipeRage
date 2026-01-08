using System;

namespace Modules.UI.Core
{
    /// <summary>
    /// Base class for all ViewModels in the MVVM architecture.
    /// Handles business logic and state for screens.
    /// </summary>
    public abstract class BaseViewModel : IDisposable
    {
        public virtual void Initialize() { }

        public virtual void Dispose() { }
    }
}