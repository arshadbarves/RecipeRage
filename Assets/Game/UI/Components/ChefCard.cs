using UnityEngine.UIElements;

namespace RecipeRage.UI
{
    /// <summary>
    /// One chef tile in the lobby/collection grid: portrait, level, lock state.
    /// </summary>
    public static class ChefCard
    {
        public static VisualElement Build(ChefDefinition chef, int level, bool unlocked, System.Action onClick)
        {
            var card = new VisualElement();
            card.AddToClassList("chef-card");
            if (!unlocked)
            {
                card.AddToClassList("chef-card-locked");
            }

            var portrait = new VisualElement();
            portrait.AddToClassList("chef-card-portrait");
            if (chef.Portrait != null)
            {
                portrait.style.backgroundImage = new StyleBackground(chef.Portrait);
            }
            card.Add(portrait);

            var label = new Label(unlocked ? $"Lv {level}" : chef.UnlockCost > 0 ? $"{chef.UnlockCost}c" : "???");
            label.AddToClassList("chef-card-label");
            card.Add(label);

            card.RegisterCallback<ClickEvent>(e => onClick());
            return card;
        }
    }
}
