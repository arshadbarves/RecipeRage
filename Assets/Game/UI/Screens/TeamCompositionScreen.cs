using Playcenter;
using Playcenter.UI;
using RecipeRage.Net;
using UnityEngine.UIElements;

namespace RecipeRage.UI
{
    /// <summary>
    /// Renders both teams from NetworkTeamRoster for the 5s composition window.
    /// Timing/transition is owned by TeamCompositionState (Slice 2).
    /// </summary>
    [UIScreen]
    public sealed class TeamCompositionScreen : BaseUIScreen
    {
        protected override void OnShow()
        {
            var roster = UnityEngine.Object.FindFirstObjectByType<NetworkTeamRoster>();
            var catalog = ServiceLocator.Get<IChefCatalog>();
            var yourTeam = Root.Q<VisualElement>("your-team");
            var enemyTeam = Root.Q<VisualElement>("enemy-team");
            yourTeam.Clear();
            enemyTeam.Clear();

            if (roster != null)
            {
                foreach (var entry in roster.Players)
                {
                    var chef = catalog.Get((ChefId)entry.ChefId);
                    var card = ChefCard.Build(chef, 1, true, () => { });
                    if (entry.TeamId == 0)
                    {
                        yourTeam.Add(card);
                    }
                    else
                    {
                        enemyTeam.Add(card);
                    }
                }
            }

            UIAnimation.SlideInFromRight(enemyTeam);
            UIAnimation.SlideInFromBottom(yourTeam);
        }
    }
}
