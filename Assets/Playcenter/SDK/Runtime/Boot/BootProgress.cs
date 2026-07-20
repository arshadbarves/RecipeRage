using System;
using System.Collections.Generic;

namespace Playcenter.SDK
{
    public sealed class BootProgress : IBootProgress
    {
        private readonly Dictionary<string, float> _weights;
        private readonly Dictionary<string, float> _progress;
        private readonly float _totalWeight;
        private string _currentModuleId;

        public event Action<float, string> Changed;

        public BootProgress(IEnumerable<(string id, float weight)> modules)
        {
            _weights = new Dictionary<string, float>();
            _progress = new Dictionary<string, float>();

            foreach (var (id, weight) in modules)
            {
                _weights[id] = weight;
                _progress[id] = 0f;
                _totalWeight += weight;
            }
        }

        public float Overall01
        {
            get
            {
                if (_totalWeight <= 0f)
                    return 1f;

                float completedWeights = 0f;
                float currentContrib = 0f;

                foreach (var kvp in _progress)
                {
                    if (!_weights.TryGetValue(kvp.Key, out float w))
                        continue;

                    if (kvp.Key == _currentModuleId)
                        currentContrib = w * kvp.Value;
                    else if (kvp.Value >= 1f)
                        completedWeights += w;
                }

                return (completedWeights + currentContrib) / _totalWeight;
            }
        }

        public void Report(string moduleId, float local01)
        {
            _progress[moduleId] = local01;
            _currentModuleId = moduleId;
            Changed?.Invoke(Overall01, moduleId);
        }
    }
}
