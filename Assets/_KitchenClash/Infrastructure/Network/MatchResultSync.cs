using Core.Logging;
using Unity.Netcode;
using UnityEngine;

namespace KitchenClash.Infrastructure.Network
{
    public enum MatchEndReason : byte
    {
        None = 0,
        TimerExpired = 1,
        ScoreLimitReached = 2
    }

    public struct MatchResultState : INetworkSerializable, System.IEquatable<MatchResultState>
    {
        public static MatchResultState None => new()
        {
            HasResult = false,
            WinningTeamId = -1,
            WinningScore = 0,
            IsDraw = false,
            EndReason = MatchEndReason.None
        };

        public bool HasResult;
        public int WinningTeamId;
        public int WinningScore;
        public bool IsDraw;
        public MatchEndReason EndReason;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref HasResult);
            serializer.SerializeValue(ref WinningTeamId);
            serializer.SerializeValue(ref WinningScore);
            serializer.SerializeValue(ref IsDraw);
            serializer.SerializeValue(ref EndReason);
        }

        public bool Equals(MatchResultState other)
        {
            return HasResult == other.HasResult
                && WinningTeamId == other.WinningTeamId
                && WinningScore == other.WinningScore
                && IsDraw == other.IsDraw
                && EndReason == other.EndReason;
        }

        public override bool Equals(object obj)
        {
            return obj is MatchResultState other && Equals(other);
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(HasResult, WinningTeamId, WinningScore, IsDraw, (int)EndReason);
        }

        public static MatchResultState FromEvaluation(MatchEndReason reason, Gameplay.GameModes.MatchEndEvaluation evaluation)
        {
            return new MatchResultState
            {
                HasResult = true,
                WinningTeamId = evaluation.WinningTeamId,
                WinningScore = evaluation.WinningScore,
                IsDraw = evaluation.IsDraw,
                EndReason = reason
            };
        }
    }

    /// <summary>
    /// Synchronized final match result snapshot published by the server once normal gameplay ends.
    /// </summary>
    public class MatchResultSync : NetworkBehaviour
    {
        private readonly NetworkVariable<MatchResultState> _currentResult = new(
            MatchResultState.None,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public MatchResultState CurrentResult => _currentResult.Value;
        public bool HasResult => _currentResult.Value.HasResult;

        public event System.Action<MatchResultState, MatchResultState> OnResultChanged;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _currentResult.OnValueChanged += HandleResultChanged;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            _currentResult.OnValueChanged -= HandleResultChanged;
        }

        public void SetResult(MatchResultState result)
        {
            if (!CanMutateResult())
            {
                return;
            }

            _currentResult.Value = result;
        }

        public void ClearResult()
        {
            if (!CanMutateResult())
            {
                return;
            }

            _currentResult.Value = MatchResultState.None;
        }

        private void HandleResultChanged(MatchResultState previousValue, MatchResultState newValue)
        {
            if (newValue.HasResult)
            {
                string resultSummary = newValue.IsDraw
                    ? $"draw at {newValue.WinningScore} points ({newValue.EndReason})"
                    : $"team {newValue.WinningTeamId} won with {newValue.WinningScore} points ({newValue.EndReason})";

                GameLogger.Log($"[MatchResultSync] Final result updated: {resultSummary}");
            }

            OnResultChanged?.Invoke(previousValue, newValue);
        }

        private bool CanMutateResult()
        {
            if (IsSpawned && !IsServer)
            {
                GameLogger.LogWarning("Only the server can mutate the final match result");
                return false;
            }

            return true;
        }
    }
}
