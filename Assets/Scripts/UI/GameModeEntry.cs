using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RecipeRage.UI
{
    /// <summary>
    /// Represents a game mode entry in the lobby UI.
    /// </summary>
    public class GameModeEntry : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _gameModeNameText;
        [SerializeField] private TextMeshProUGUI _gameModeDescriptionText;
        [SerializeField] private Image _gameModeIconImage;
        [SerializeField] private Button _selectButton;
        
        /// <summary>
        /// The ID of the game mode.
        /// </summary>
        private string _gameModeId;
        
        /// <summary>
        /// Set the game mode name.
        /// </summary>
        /// <param name="name">The game mode name</param>
        public void SetGameModeName(string name)
        {
            if (_gameModeNameText != null)
            {
                _gameModeNameText.text = name;
            }
        }
        
        /// <summary>
        /// Set the game mode description.
        /// </summary>
        /// <param name="description">The game mode description</param>
        public void SetGameModeDescription(string description)
        {
            if (_gameModeDescriptionText != null)
            {
                _gameModeDescriptionText.text = description;
            }
        }
        
        /// <summary>
        /// Set the game mode icon.
        /// </summary>
        /// <param name="icon">The game mode icon sprite</param>
        public void SetGameModeIcon(Sprite icon)
        {
            if (_gameModeIconImage != null && icon != null)
            {
                _gameModeIconImage.sprite = icon;
            }
        }
        
        /// <summary>
        /// Set the game mode ID.
        /// </summary>
        /// <param name="id">The game mode ID</param>
        public void SetGameModeId(string id)
        {
            _gameModeId = id;
        }
        
        /// <summary>
        /// Get the game mode ID.
        /// </summary>
        /// <returns>The game mode ID</returns>
        public string GetGameModeId()
        {
            return _gameModeId;
        }
        
        /// <summary>
        /// Set the select button click listener.
        /// </summary>
        /// <param name="onClick">The click listener</param>
        public void SetSelectButtonListener(System.Action onClick)
        {
            if (_selectButton != null)
            {
                _selectButton.onClick.RemoveAllListeners();
                _selectButton.onClick.AddListener(() => onClick?.Invoke());
            }
        }
    }
}
