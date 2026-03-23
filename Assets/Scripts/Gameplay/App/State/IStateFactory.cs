using System;

namespace Gameplay.App.State
{
    /// <summary>
    /// Creates state instances from the application container.
    /// </summary>
    public interface IStateFactory
    {
        T Create<T>() where T : IState;
        IState Create(Type stateType);
    }
}
