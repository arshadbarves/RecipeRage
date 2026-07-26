using System;
using Playcenter.UI.Toolkit;
using VContainer;

namespace KitchenClash.Presentation.Common
{
    /// <summary>
    /// VContainer-backed implementation of <see cref="IScreenInstanceFactory"/>.
    /// Resolves screen instances from the active DI scope.
    /// Implements <see cref="IScopeAwareScreenFactory"/> so the session scope can be swapped
    /// at runtime via <see cref="UIService.SetCurrentScope"/>.
    /// </summary>
    public sealed class VContainerScreenInstanceFactory : IScreenInstanceFactory, IScopeAwareScreenFactory
    {
        private IObjectResolver _resolver;

        public VContainerScreenInstanceFactory(IObjectResolver resolver)
        {
            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        }

        public void SetScope(object scope)
        {
            if (scope is IObjectResolver resolver)
                _resolver = resolver;
        }

        public object Create(Type screenType)
        {
            return _resolver.Resolve(screenType);
        }
    }
}
