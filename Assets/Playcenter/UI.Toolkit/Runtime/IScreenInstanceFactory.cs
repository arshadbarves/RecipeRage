using System;

namespace Playcenter.UI.Toolkit
{
    /// <summary>
    /// Creates screen instances without exposing a DI container type.
    /// Game supplies a DI-backed implementation.
    /// </summary>
    public interface IScreenInstanceFactory
    {
        object Create(Type screenType);
    }
}
