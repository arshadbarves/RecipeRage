using Gameplay.Cooking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// UI item for displaying an order.
    /// </summary>
    public class OrderUIItem : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI _recipeName;
        [SerializeField] private Image _recipeIcon;
        [SerializeField] private TextMeshProUGUI _timeRemaining;
        [SerializeField] private Image _timerFill;
        [SerializeField] private GameObject _ingredientIconPrefab;
        [SerializeField] private Transform _ingredientIconsContainer;
        
        /// <summary>
        /// The order state.
        /// </summary>
        private RecipeOrderState _order;
        
        /// <summary>
        /// The recipe.
        /// </summary>
        private Recipe _recipe;
        
        /// <summary>
        /// Set the order data.
        /// </summary>
        /// <param name="order">The order state.</param>
        /// <param name="recipe">The recipe.</param>
        public void SetOrder(RecipeOrderState order, Recipe recipe)
        {
            _order = order;
            _recipe = recipe;
            
            // Set the recipe name
            if (_recipeName != null)
            {
                _recipeName.text = recipe.DisplayName;
            }
            
            // Set the recipe icon
            if (_recipeIcon != null && recipe.Icon != null)
            {
                _recipeIcon.sprite = recipe.Icon;
                _recipeIcon.gameObject.SetActive(true);
            }
            else if (_recipeIcon != null)
            {
                _recipeIcon.gameObject.SetActive(false);
            }
            
            // Create ingredient icons
            if (_ingredientIconPrefab != null && _ingredientIconsContainer != null)
            {
                // Clear existing icons
                foreach (Transform child in _ingredientIconsContainer)
                {
                    Destroy(child.gameObject);
                }
                
                // Create new icons
                foreach (RecipeIngredient ingredient in recipe.Ingredients)
                {
                    GameObject iconObject = Instantiate(_ingredientIconPrefab, _ingredientIconsContainer);
                    
                    // Set the icon
                    Image iconImage = iconObject.GetComponent<Image>();
                    if (iconImage != null && ingredient.Ingredient.Icon != null)
                    {
                        iconImage.sprite = ingredient.Ingredient.Icon;
                        iconImage.color = ingredient.Ingredient.Color;
                    }
                }
            }
        }
        
        /// <summary>
        /// Update the order UI item.
        /// </summary>
        private void Update()
        {
            if (_recipe == null)
            {
                return;
            }
            
            // Calculate time remaining
            float timeElapsed = Time.time - _order.CreationTime;
            float timeRemaining = _order.TimeLimit - timeElapsed;
            
            // Update the time remaining text
            if (_timeRemaining != null)
            {
                int seconds = Mathf.Max(0, Mathf.FloorToInt(timeRemaining));
                _timeRemaining.text = $"{seconds}s";
                
                // Change color based on time remaining
                if (timeRemaining < _order.TimeLimit * 0.25f)
                {
                    _timeRemaining.color = Color.red;
                }
                else if (timeRemaining < _order.TimeLimit * 0.5f)
                {
                    _timeRemaining.color = Color.yellow;
                }
                else
                {
                    _timeRemaining.color = Color.white;
                }
            }
            
            // Update the timer fill
            if (_timerFill != null)
            {
                _timerFill.fillAmount = Mathf.Max(0f, timeRemaining / _order.TimeLimit);
                
                // Change color based on time remaining
                if (timeRemaining < _order.TimeLimit * 0.25f)
                {
                    _timerFill.color = Color.red;
                }
                else if (timeRemaining < _order.TimeLimit * 0.5f)
                {
                    _timerFill.color = Color.yellow;
                }
                else
                {
                    _timerFill.color = Color.green;
                }
            }
        }
    }
}
