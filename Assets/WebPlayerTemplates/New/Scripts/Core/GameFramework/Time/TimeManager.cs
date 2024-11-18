using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Core.GameFramework.Time
{
    public class NetworkTimeManager : NetworkBehaviour, ITimeManager
    {

        private const float PingInterval = 1f;
        private const float MaxPredictionTime = 0.1f; // 100ms
        private readonly List<Timer> _activeTimers = new List<Timer>();
        private readonly float[] _latencyBuffer = new float[30];
        private readonly NetworkVariable<NetworkTimeData> _networkTimeData = new NetworkVariable<NetworkTimeData>();
        private readonly Queue<TimePing> _pendingPings = new Queue<TimePing>();
        private readonly List<Timer> _timersToAdd = new List<Timer>();
        private readonly List<Timer> _timersToRemove = new List<Timer>();
        private double _lastServerTime;
        private int _latencyBufferIndex;

        private double _localGameTime;
        private float _pingTimer;

        private void Update()
        {
            if (!IsSpawned) return;

            UpdateGameTime();
            UpdateTimers();

            if (IsClient)
            {
                UpdatePings();
            }
        }

        public float GameTime => (float)_localGameTime;
        public float DeltaTime => IsPaused ? 0f : UnityEngine.Time.deltaTime * TimeScale;
        public float FixedDeltaTime => IsPaused ? 0f : UnityEngine.Time.fixedDeltaTime * TimeScale;
        public float TimeScale => _networkTimeData.Value.TimeScale;
        public bool IsPaused => _networkTimeData.Value.IsPaused;
        public float Latency { get; private set; }

        public float PredictedTime => (float)(_localGameTime + Latency * 0.5f);

        public Timer CreateTimer(float duration, bool isLooping = false)
        {
            return CreateTimer(duration, null, null, isLooping);
        }

        public Timer CreateTimer(float duration, Action onComplete, bool isLooping = false)
        {
            return CreateTimer(duration, null, onComplete, isLooping);
        }

        public Timer CreateTimer(float duration, Action<float> onUpdate, Action onComplete, bool isLooping = false)
        {
            Timer timer = new Timer(duration, onComplete, onUpdate, isLooping);
            _timersToAdd.Add(timer);
            return timer;
        }

        public void UpdateTimers()
        {
            if (_timersToAdd.Count > 0)
            {
                _activeTimers.AddRange(_timersToAdd);
                _timersToAdd.Clear();
            }

            for (int i = _activeTimers.Count - 1; i >= 0; i--)
            {
                if (_activeTimers[i].Update(DeltaTime))
                {
                    _timersToRemove.Add(_activeTimers[i]);
                }
            }

            if (_timersToRemove.Count > 0)
            {
                foreach (Timer timer in _timersToRemove)
                {
                    _activeTimers.Remove(timer);
                }
                _timersToRemove.Clear();
            }
        }

        public void CancelAllTimers()
        {
            foreach (Timer timer in _activeTimers)
            {
                timer.Cancel();
            }
            _activeTimers.Clear();
            _timersToAdd.Clear();
            _timersToRemove.Clear();
        }

        public float GetPredictedTimeForAction(float actionDuration)
        {
            float predictedLatency = Mathf.Min(Latency, MaxPredictionTime);
            return (float)_localGameTime + actionDuration + predictedLatency;
        }

        public Timer Schedule(float delay, Action action)
        {
            return CreateTimer(delay, action);
        }

        public Timer ScheduleRepeating(float interval, Action action)
        {
            return CreateTimer(interval, null, action, true);
        }

        public double GetAdjustedServerTime()
        {
            return _networkTimeData.Value.ServerTime + Latency * 0.5f;
        }

        public double LocalToServerTime(double localTime)
        {
            return localTime + Latency * 0.5f;
        }

        public double ServerToLocalTime(double serverTime)
        {
            return serverTime - Latency * 0.5f;
        }

        public void Pause()
        {
            if (IsServer)
            {
                SetPaused(true);
            }
            else
            {
                PauseServerRpc();
            }
        }

        public void Resume()
        {
            if (IsServer)
            {
                SetPaused(false);
            }
            else
            {
                ResumeServerRpc();
            }
        }

        public void SetTimeScale(float scale)
        {
            if (IsServer)
            {
                SetTimeScaleInternal(scale);
            }
            else
            {
                SetTimeScaleServerRpc(scale);
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsServer)
            {
                NetworkTimeData timeData = _networkTimeData.Value;
                timeData.ServerTime = 0;
                timeData.TimeScale = 1f;
                timeData.IsPaused = false;
                _networkTimeData.Value = timeData;
            }

            if (IsClient)
            {
                _pingTimer = PingInterval;
            }
        }

        private void UpdateGameTime()
        {
            if (!IsPaused)
            {
                float deltaTime = UnityEngine.Time.deltaTime * TimeScale;

                if (IsServer)
                {
                    _localGameTime += deltaTime;
                    UpdateNetworkTime();
                }
                else
                {
                    // Client time synchronization with prediction
                    double serverTime = _networkTimeData.Value.ServerTime;
                    double predictedServerTime = serverTime + Latency * 0.5f;

                    double timeError = predictedServerTime - _localGameTime;

                    if (Mathf.Abs((float)timeError) > 0.001f)
                    {
                        double correction = timeError * deltaTime * 5f;
                        correction = Mathf.Clamp((float)correction, -0.1f, 0.1f);
                        _localGameTime += deltaTime + correction;
                    }
                    else
                    {
                        _localGameTime += deltaTime;
                    }

                    _lastServerTime = serverTime;
                }
            }
        }

        private void UpdatePings()
        {
            if (_pingTimer <= 0f)
            {
                SendPing();
                _pingTimer = PingInterval;
            }
            _pingTimer -= UnityEngine.Time.deltaTime;
        }

        private void SendPing()
        {
            uint sequenceId = (uint)Random.Range(0, uint.MaxValue);
            TimePing ping = new TimePing {
                SendTime = _localGameTime, SequenceId = sequenceId
            };

            _pendingPings.Enqueue(ping);
            PingServerRpc(sequenceId, _localGameTime);
        }

        [ServerRpc(RequireOwnership = false)]
        private void PingServerRpc(uint sequenceId, double clientTime)
        {
            PongClientRpc(sequenceId, clientTime, _localGameTime);
        }

        [ClientRpc]
        private void PongClientRpc(uint sequenceId, double originalClientTime, double serverTime)
        {
            while (_pendingPings.Count > 0)
            {
                TimePing ping = _pendingPings.Peek();
                if (ping.SequenceId == sequenceId)
                {
                    double roundTripTime = _localGameTime - originalClientTime;
                    UpdateLatency((float)roundTripTime * 0.5f);
                    _pendingPings.Dequeue();
                    break;
                }
                if (ping.SendTime < originalClientTime)
                {
                    _pendingPings.Dequeue();
                }
                else
                {
                    break;
                }
            }
        }

        private void UpdateLatency(float newLatency)
        {
            _latencyBuffer[_latencyBufferIndex] = newLatency;
            _latencyBufferIndex = (_latencyBufferIndex + 1) % _latencyBuffer.Length;

            float sum = 0f;
            int count = 0;
            for (int i = 0; i < _latencyBuffer.Length; i++)
            {
                if (_latencyBuffer[i] > 0)
                {
                    sum += _latencyBuffer[i];
                    count++;
                }
            }
            Latency = count > 0 ? sum / count : 0f;
        }

        private void UpdateNetworkTime()
        {
            NetworkTimeData timeData = _networkTimeData.Value;
            timeData.ServerTime = _localGameTime;
            _networkTimeData.Value = timeData;
        }

        [ServerRpc(RequireOwnership = false)]
        private void PauseServerRpc()
        {
            SetPaused(true);
        }

        [ServerRpc(RequireOwnership = false)]
        private void ResumeServerRpc()
        {
            SetPaused(false);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SetTimeScaleServerRpc(float scale)
        {
            SetTimeScaleInternal(scale);
        }

        private void SetPaused(bool paused)
        {
            NetworkTimeData timeData = _networkTimeData.Value;
            timeData.IsPaused = paused;
            _networkTimeData.Value = timeData;
        }

        private void SetTimeScaleInternal(float scale)
        {
            NetworkTimeData timeData = _networkTimeData.Value;
            timeData.TimeScale = scale;
            _networkTimeData.Value = timeData;
        }

        private struct TimePing
        {
            public double SendTime;
            public uint SequenceId;
        }
    }
}