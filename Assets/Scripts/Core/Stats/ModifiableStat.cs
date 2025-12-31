using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Stats
{
    [Serializable]
    public class ModifiableStat
    {
        [SerializeField]
        private float _baseValue;

        private bool _isDirty = true;
        private float _value;
        private readonly List<StatModifier> _modifiers = new();

        public float BaseValue
        {
            get => _baseValue;
            set
            {
                _baseValue = value;
                _isDirty = true;
            }
        }

        public float Value
        {
            get
            {
                if (_isDirty)
                {
                    _value = CalculateFinalValue();
                    _isDirty = false;
                }
                return _value;
            }
        }

        public ModifiableStat(float baseValue = 0f)
        {
            _baseValue = baseValue;
        }

        public void AddModifier(StatModifier mod)
        {
            _modifiers.Add(mod);
            _isDirty = true;
        }

        public bool RemoveModifier(StatModifier mod)
        {
            if (_modifiers.Remove(mod))
            {
                _isDirty = true;
                return true;
            }
            return false;
        }

        public bool RemoveAllModifiersFromSource(object source)
        {
            int numRemoved = _modifiers.RemoveAll(mod => mod.Source == source);
            if (numRemoved > 0)
            {
                _isDirty = true;
                return true;
            }
            return false;
        }

        private float CalculateFinalValue()
        {
            float finalValue = _baseValue;
            float sumPercentAdd = 0;

            for (int i = 0; i < _modifiers.Count; i++)
            {
                StatModifier mod = _modifiers[i];

                if (mod.Type == StatModifierType.Flat)
                {
                    finalValue += mod.Value;
                }
                else if (mod.Type == StatModifierType.PercentAdd)
                {
                    sumPercentAdd += mod.Value;
                }
            }

            finalValue *= 1 + sumPercentAdd;

            for (int i = 0; i < _modifiers.Count; i++)
            {
                StatModifier mod = _modifiers[i];

                if (mod.Type == StatModifierType.PercentMult)
                {
                    finalValue *= mod.Value;
                }
            }

            return (float)Math.Round(finalValue, 4);
        }
    }
}