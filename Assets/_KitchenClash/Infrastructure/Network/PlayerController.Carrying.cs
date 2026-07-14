using System.Collections.Generic;
using UnityEngine;
using KitchenClash.Domain;
using KitchenClash.Domain.Enums;


namespace KitchenClash.Infrastructure.Network
{
    public partial class PlayerController
    {
        #region Object Carrying

        public bool PickUpObject(GameObject obj)
        {
            if (_heldObject != null || _holdPoint == null)
            {
                return false;
            }

            _heldObject = obj;
            obj.transform.SetParent(_holdPoint);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;

            Rigidbody objRigidbody = obj.GetComponent<Rigidbody>();
            if (objRigidbody != null)
            {
                objRigidbody.isKinematic = true;
            }

            return true;
        }

        public GameObject DropObject()
        {
            if (_heldObject == null)
            {
                return null;
            }

            GameObject obj = _heldObject;
            _heldObject = null;
            obj.transform.SetParent(null);

            Rigidbody objRigidbody = obj.GetComponent<Rigidbody>();
            if (objRigidbody != null)
            {
                objRigidbody.isKinematic = false;
            }

            return obj;
        }

        public GameObject GetHeldObject() => _heldObject;
        public bool IsHoldingObject() => _heldObject != null;

        /// <summary>True when carried dish slots are full (capacity from CarryingCapacity, min 1).</summary>
        public bool IsCarryingMaxItems => _carriedDishes.Count >= GetMaxCarrySlots();

        /// <summary>True when the player is holding at least one dish for delivery.</summary>
        public bool HasCarriedDish => _carriedDishes.Count > 0;

        /// <summary>
        /// Server-side: add a collected dish to the carry list (station collect / loot / steal).
        /// </summary>
        public void ReceiveCollectedDish(int recipeTier, IngredientType ingredientType)
        {
            if (IsCarryingMaxItems)
            {
                return;
            }

            _carriedDishes.Add(new CarriedItemData(ingredientType, recipeTier));
        }

        /// <summary>
        /// Consume one carried dish for delivery. Returns false if none carried.
        /// </summary>
        public bool TryConsumeCarriedDish(out CarriedItemData dish)
        {
            if (_carriedDishes.Count == 0)
            {
                dish = default;
                return false;
            }

            int last = _carriedDishes.Count - 1;
            dish = _carriedDishes[last];
            _carriedDishes.RemoveAt(last);
            return true;
        }

        /// <summary>
        /// Clear and return all carried dishes (KO loot drop / Disruptor steal).
        /// </summary>
        public List<CarriedItemData> GetAndClearCarriedItems()
        {
            if (_carriedDishes.Count == 0)
            {
                return new List<CarriedItemData>(0);
            }

            var copy = new List<CarriedItemData>(_carriedDishes);
            _carriedDishes.Clear();
            return copy;
        }

        private int GetMaxCarrySlots()
        {
            int capacity = Mathf.RoundToInt(CarryingCapacity.CurrentValue);
            return Mathf.Max(1, capacity);
        }

        #endregion

    }
}
