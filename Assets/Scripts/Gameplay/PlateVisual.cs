using System.Collections.Generic;
using Core;
using Gameplay.Data;
using GameSystem.Gameplay;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay
{
    public class PlateVisual : NetworkBehaviour
    {
        [SerializeField] private GameObject platePrefab;
        [SerializeField] private Transform ingredientContainer;

        private readonly List<IngredientVisual> _ingredients = new List<IngredientVisual>();

        [ServerRpc(RequireOwnership = false)]
        public void AddIngredientServerRpc(string ingredientID)
        {
            IngredientData ingredientData = GetIngredientDataByID(ingredientID);
            if (ingredientData == null)
            {
                Debug.LogError($"IngredientData with ID {ingredientID} not found.");
                return;
            }

            GameObject ingredientObj =
                Instantiate(ingredientData.IngredientStates.Find(pair => pair.State == IngredientState.Raw)?.GameObject,
                    ingredientContainer);
            
            IngredientVisual ingredient = ingredientObj.GetComponent<IngredientVisual>();
            ingredient.Initialize(ingredientData);
            _ingredients.Add(ingredient);

            ingredientObj.GetComponent<NetworkObject>().Spawn();
            UpdatePlateVisualClientRpc();
        }

        [ClientRpc]
        private void UpdatePlateVisualClientRpc()
        {
            ArrangeIngredientsOnPlate();
        }

        private void ArrangeIngredientsOnPlate()
        {
            float radius = 0.5f;
            int ingredientCount = _ingredients.Count;

            for (int i = 0; i < ingredientCount; i++)
            {
                float angle = i * Mathf.PI * 2f / ingredientCount;
                Vector3 newPos = new Vector3(Mathf.Cos(angle) * radius, 0.1f, Mathf.Sin(angle) * radius);
                _ingredients[i].transform.localPosition = newPos;
            }
        }

        private void SpawnPlatePrefab()
        {
            GameObject plateObj = Instantiate(platePrefab, transform);
            plateObj.GetComponent<NetworkObject>().Spawn();
        }

        private IngredientData GetIngredientDataByID(string ingredientID)
        {
            IngredientData ingredientData = null;
            if (GameManager.Instance.GetSystem<GameplaySystem>().CurrentGameMode != null)
            {
                ingredientData = GameManager.Instance.GetSystem<GameplaySystem>().CurrentGameMode.GetIngredientData(ingredientID);
            }
            
            return ingredientData;
        }
    }
}