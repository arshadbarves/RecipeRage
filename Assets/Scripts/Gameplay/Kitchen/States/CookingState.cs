using UnityEngine;
using Unity.Netcode;
using RecipeRage.Gameplay.Core.States;

namespace RecipeRage.Gameplay.Kitchen.States
{
    public class CookingState : NetworkState
    {
        public float Progress;
        public float Quality;
        public float BurnTimer;
        public bool IsAttended;

        private StateType _type;
        private readonly float _optimalCookTime;
        private readonly float _burnTime;

        public override StateType Type => _type;

        public CookingState(StateType type, float optimalCookTime, float burnTime)
        {
            _type = type;
            _optimalCookTime = optimalCookTime;
            _burnTime = burnTime;
            Progress = 0f;
            Quality = 1f;
            BurnTimer = 0f;
            IsAttended = false;
        }

        public override void OnUpdate()
        {
            StateTime += Time.deltaTime;

            switch (_type)
            {
                case StateType.Raw:
                    // No updates needed in raw state
                    break;

                case StateType.Cooking:
                    UpdateCooking();
                    break;

                case StateType.Cooked:
                    UpdateCooked();
                    break;

                case StateType.Burnt:
                    // No updates needed in burnt state
                    break;
            }
        }

        private void UpdateCooking()
        {
            // Update progress
            Progress = Mathf.Clamp01(StateTime / _optimalCookTime);

            // Update quality based on attention
            if (!IsAttended)
            {
                Quality = Mathf.Max(0.5f, Quality - Time.deltaTime * 0.1f);
            }
            else
            {
                Quality = Mathf.Min(1f, Quality + Time.deltaTime * 0.05f);
            }
        }

        private void UpdateCooked()
        {
            // Start burning if left too long
            BurnTimer += Time.deltaTime;
            if (BurnTimer >= _burnTime)
            {
                _type = StateType.Burnt;
                Quality = 0f;
            }
            else
            {
                // Gradually decrease quality while cooked
                Quality = Mathf.Max(0.1f, Quality - Time.deltaTime * 0.05f);
            }
        }

        public override void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            base.NetworkSerialize(serializer);
            serializer.SerializeValue(ref Progress);
            serializer.SerializeValue(ref Quality);
            serializer.SerializeValue(ref BurnTimer);
            serializer.SerializeValue(ref IsAttended);
        }

        public override bool Equals(NetworkState other)
        {
            if (!base.Equals(other)) return false;
            if (!(other is CookingState state)) return false;
            
            return Progress.Equals(state.Progress) &&
                   Quality.Equals(state.Quality) &&
                   BurnTimer.Equals(state.BurnTimer) &&
                   IsAttended == state.IsAttended;
        }
    }
}
