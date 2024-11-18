using Unity.Netcode;
using System;

namespace RecipeRage.Gameplay.Core.States
{
    public abstract class NetworkState : INetworkSerializable, IEquatable<NetworkState>
    {
        public float StateTime { get; protected set; }
        public abstract StateType Type { get; }

        public virtual void OnEnter() {}
        public virtual void OnExit() {}
        public virtual void OnUpdate() {}
        
        public virtual void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            float stateTime = StateTime;
            serializer.SerializeValue(ref stateTime);
        }

        public virtual bool Equals(NetworkState other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Type == other.Type && StateTime.Equals(other.StateTime);
        }
    }

    public enum StateType
    {
        None,
        // Character States
        Idle,
        Moving,
        Attacking,
        Stunned,
        Dead,
        
        // Cooking States
        Raw,
        Cooking,
        Cooked,
        Burnt,
        
        // Item States
        Available,
        InUse,
        Depleted
    }
}
