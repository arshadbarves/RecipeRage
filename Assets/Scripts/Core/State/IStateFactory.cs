using System;

namespace Core.State
{
    /// <summary>
    /// Factory for creating game states with dependency injection support.
    /// </summary>
    public interface IStateFactory
    {
        T CreateState<T>() where T : IState;
    }
}
