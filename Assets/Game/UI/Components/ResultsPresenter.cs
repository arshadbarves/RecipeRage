using Playcenter;
using Playcenter.Services;
using Playcenter.UI;

namespace RecipeRage.UI
{
    /// <summary>
    /// Translates MatchEndedEvent into the ResultsScreen. Registered by
    /// GameplayCompositionRoot; computes coins (50/20 + 5/recipe) and
    /// trophy delta (+15/-8) from the event.
    /// </summary>
    public sealed class ResultsPresenter
    {
        public void Initialize(IEventBus eventBus, IUIService ui)
        {
            eventBus.Subscribe<MatchEndedEvent>(e =>
            {
                var coins = (e.Won ? 50 : 20) + e.TeamRecipes * 5;
                var trophyDelta = e.Won ? 15 : -8;

                ui.Show<ResultsScreen>();
                if (ui.Current is ResultsScreen results)
                {
                    results.SetResults(e.Won, e.TeamRecipes, e.EnemyRecipes, coins, trophyDelta);
                }
            });
        }
    }
}
