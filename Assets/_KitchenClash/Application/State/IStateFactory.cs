using System;

namespace KitchenClash.Application.State
{
    public interface IStateFactory
    {
        T Create<T>() where T : IState;
        IState Create(Type stateType);
    }
}
