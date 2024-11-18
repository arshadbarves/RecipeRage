using System.Collections.Generic;

namespace Gameplay.Character.Stats
{
    public class StatValue
    {
        private readonly List<StatModifier> _modifiers = new List<StatModifier>();
        private float _baseValue;
        private float _cachedValue;
        private bool _isDirty = true;

        public float BaseValue {
            get => _baseValue;
            set {
                _baseValue = value;
                MarkDirty();
            }
        }

        public float Value {
            get {
                if (_isDirty)
                {
                    _cachedValue = CalculateFinalValue();
                    _isDirty = false;
                }
                return _cachedValue;
            }
        }

        public void AddModifier(StatModifier modifier)
        {
            _modifiers.Add(modifier);
            _modifiers.Sort((a, b) => a.Order.CompareTo(b.Order));
            MarkDirty();
        }

        public bool RemoveModifier(StatModifier modifier)
        {
            if (_modifiers.Remove(modifier))
            {
                MarkDirty();
                return true;
            }
            return false;
        }

        public void RemoveAllModifiersFromSource(object source)
        {
            for (int i = _modifiers.Count - 1; i >= 0; i--)
            {
                if (_modifiers[i].Source == source)
                {
                    _modifiers.RemoveAt(i);
                    MarkDirty();
                }
            }
        }

        private float CalculateFinalValue()
        {
            float finalValue = _baseValue;
            float sumPercentAdd = 0;

            for (int i = 0; i < _modifiers.Count; i++)
            {
                StatModifier mod = _modifiers[i];

                if (mod.Type == ModifierType.Flat)
                {
                    finalValue += mod.Value;
                }
                else if (mod.Type == ModifierType.PercentAdd)
                {
                    sumPercentAdd += mod.Value;

                    if (i + 1 >= _modifiers.Count || _modifiers[i + 1].Order != mod.Order)
                    {
                        finalValue *= 1 + sumPercentAdd;
                        sumPercentAdd = 0;
                    }
                }
                else // PercentMult
                {
                    finalValue *= 1 + mod.Value;
                }
            }

            return finalValue;
        }

        private void MarkDirty()
        {
            _isDirty = true;
        }
    }
}