using System;
using System.Collections.Generic;
using Playcenter;
using Playcenter.Services;

namespace RecipeRage
{
    /// <summary>
    /// Runs one match: recipe list, countdown timer, plate validation, win condition.
    /// Single-player scope — enemy team is simulated at 0 for now; Slice 2 syncs real teams.
    /// </summary>
    public sealed class MatchController
    {
        private readonly IRecipeCatalog _catalog;
        private readonly IConfigService _config;
        private readonly IEventBus _eventBus;
        private readonly ITimeService _time;

        private MatchState _state;
        private float _matchDuration;

        public event Action OnRecipeCompleted;
        public event Action<bool> OnMatchEnded;

        public RecipeDefinition CurrentRecipe =>
            _state != null && _state.CurrentIndex < _state.RecipeList.Count
                ? _state.RecipeList[_state.CurrentIndex]
                : null;

        public string CurrentRecipeId => CurrentRecipe != null ? CurrentRecipe.Id : string.Empty;
        public int CompletedCount => _state?.CurrentIndex ?? 0;
        public int TotalCount => _state?.RecipeList.Count ?? 0;
        public float RemainingSeconds => _state?.RemainingSeconds ?? 0f;
        public bool IsMatchOver => _state?.IsOver ?? true;

        public MatchController(IRecipeCatalog catalog, IConfigService config, IEventBus eventBus, ITimeService time)
        {
            _catalog = catalog;
            _config = config;
            _eventBus = eventBus;
            _time = time;
        }

        public void StartMatch(int seed)
        {
            var easy = _config.Get(ConfigKeys.RecipesEasy2v2, ConfigKeys.Defaults.RecipesEasy2v2);
            var medium = _config.Get(ConfigKeys.RecipesMedium2v2, ConfigKeys.Defaults.RecipesMedium2v2);
            var hard = _config.Get(ConfigKeys.RecipesHard2v2, ConfigKeys.Defaults.RecipesHard2v2);
            _matchDuration = _config.Get(ConfigKeys.MatchDurationSec, ConfigKeys.Defaults.MatchDurationSec);

            _state = new MatchState
            {
                RecipeList = _catalog.GetRandomRecipeList(easy, medium, hard, seed),
                CurrentIndex = 0,
                RemainingSeconds = _matchDuration,
                IsOver = false
            };

            _eventBus.Publish(new MatchStartedEvent());
        }

        public void Tick()
        {
            if (_state == null || _state.IsOver)
            {
                return;
            }

            _state.RemainingSeconds -= _time.DeltaTime;
            if (_state.RemainingSeconds <= 0f)
            {
                EndMatch(false);
            }
        }

        public bool TryServePlate(Plate plate)
        {
            if (_state == null || _state.IsOver || CurrentRecipe == null)
            {
                return false;
            }

            if (!ValidatePlate(plate, CurrentRecipe))
            {
                return false;
            }

            _state.CurrentIndex++;
            OnRecipeCompleted?.Invoke();

            if (_state.CurrentIndex >= _state.RecipeList.Count)
            {
                EndMatch(true);
            }
            return true;
        }

        private void EndMatch(bool completedAll)
        {
            _state.IsOver = true;
            _eventBus.Publish(new MatchEndedEvent(completedAll, _state.CurrentIndex, 0));
            OnMatchEnded?.Invoke(completedAll);
        }

        private static bool ValidatePlate(Plate plate, RecipeDefinition recipe)
        {
            var requirements = recipe.RequiredIngredients;
            if (plate.Contents.Count != requirements.Length)
            {
                return false;
            }

            var used = new bool[plate.Contents.Count];
            foreach (var requirement in requirements)
            {
                var matched = false;
                for (int i = 0; i < plate.Contents.Count; i++)
                {
                    if (used[i])
                    {
                        continue;
                    }

                    var item = plate.Contents[i];
                    if (item.Definition.Type != requirement.Type || item.IsBurnt)
                    {
                        continue;
                    }
                    if (requirement.RequiresChopped && !item.IsChopped)
                    {
                        continue;
                    }
                    if (requirement.RequiresCooked && !item.IsCooked)
                    {
                        continue;
                    }

                    used[i] = true;
                    matched = true;
                    break;
                }

                if (!matched)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
