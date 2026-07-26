using Playcenter;
using Playcenter.UI;
using RecipeRage.Net;
using UnityEngine.UIElements;

namespace RecipeRage.UI
{
    /// <summary>
    /// Minimal HUD: team recipe count, timer, enemy count, current recipe
    /// checklist. Points are never displayed — completion is the goal.
    /// </summary>
    [UIScreen]
    public sealed class HUDScreen : BaseUIScreen
    {
        private Label _teamCount;
        private Label _enemyCount;
        private Label _timer;
        private VisualElement _checklist;
        private NetworkMatch _networkMatch;

        protected override void OnShow()
        {
            _teamCount = Root.Q<Label>("team-count");
            _enemyCount = Root.Q<Label>("enemy-count");
            _timer = Root.Q<Label>("match-timer");
            _checklist = Root.Q<VisualElement>("recipe-checklist");

            _networkMatch = UnityEngine.Object.FindFirstObjectByType<NetworkMatch>();
            RefreshChecklist();
        }

        private void Update()
        {
            if (_networkMatch == null || !_networkMatch.IsSpawned)
            {
                return;
            }

            var localTeam = 0; // from local player's NetworkPlayer.TeamId
            _teamCount.text = $"{(localTeam == 0 ? _networkMatch.TeamACompleted.Value : _networkMatch.TeamBCompleted.Value)}/{GetTotal()}";
            _enemyCount.text = $"{(localTeam == 0 ? _networkMatch.TeamBCompleted.Value : _networkMatch.TeamACompleted.Value)}/{GetTotal()}";

            var remaining = _networkMatch.RemainingSeconds.Value;
            _timer.text = $"{(int)(remaining / 60)}:{(int)(remaining % 60):00}";
            if (remaining < 30f)
            {
                UIAnimation.ScalePulse(_timer, 0.5f);
            }
        }

        private int GetTotal()
        {
            var config = ServiceLocator.Get<Playcenter.Services.IConfigService>();
            return config.Get(ConfigKeys.RecipesEasy2v2, ConfigKeys.Defaults.RecipesEasy2v2)
                 + config.Get(ConfigKeys.RecipesMedium2v2, ConfigKeys.Defaults.RecipesMedium2v2)
                 + config.Get(ConfigKeys.RecipesHard2v2, ConfigKeys.Defaults.RecipesHard2v2);
        }

        private void RefreshChecklist()
        {
            _checklist.Clear();
            var match = ServiceLocator.Get<MatchController>();
            var recipe = match?.CurrentRecipe;
            if (recipe == null)
            {
                return;
            }

            foreach (var requirement in recipe.RequiredIngredients)
            {
                var item = new Label($"☐ {requirement.Type}");
                item.AddToClassList("recipe-checklist-item");
                _checklist.Add(item);
            }
        }
    }
}
