using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Represents a player entry in the lobby UI.
    /// </summary>
    public class LobbyPlayerEntry : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _playerNameText;
        [SerializeField] private Image _readyStatusImage;
        [SerializeField] private Image _playerIconImage;
        [SerializeField] private TextMeshProUGUI _playerLevelText;
        
        /// <summary>
        /// Set the player name.
        /// </summary>
        /// <param name="playerName">The player name</param>
        public void SetPlayerName(string playerName)
        {
            if (_playerNameText != null)
            {
                _playerNameText.text = playerName;
            }
        }
        
        /// <summary>
        /// Set the ready status.
        /// </summary>
        /// <param name="isReady">Whether the player is ready</param>
        public void SetReadyStatus(bool isReady)
        {
            if (_readyStatusImage != null)
            {
                _readyStatusImage.color = isReady ? Color.green : Color.red;
            }
        }
        
        /// <summary>
        /// Set the player icon.
        /// </summary>
        /// <param name="icon">The player icon sprite</param>
        public void SetPlayerIcon(Sprite icon)
        {
            if (_playerIconImage != null && icon != null)
            {
                _playerIconImage.sprite = icon;
            }
        }
        
        /// <summary>
        /// Set the player level.
        /// </summary>
        /// <param name="level">The player level</param>
        public void SetPlayerLevel(int level)
        {
            if (_playerLevelText != null)
            {
                _playerLevelText.text = $"Lvl {level}";
            }
        }
    }
}
