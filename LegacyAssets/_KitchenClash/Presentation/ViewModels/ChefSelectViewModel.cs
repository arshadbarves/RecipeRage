using KitchenClash.Application.Services;
using KitchenClash.Domain;

namespace KitchenClash.Presentation.ViewModels
{
    public sealed class ChefSelectViewModel : ScreenViewModel
    {
        public BindableProperty<ChefId> SelectedChef { get; } = new(ChefId.Rosa);
        public BindableProperty<string> ChefName { get; } = new("Rosa");
        public BindableProperty<string> PassiveDescription { get; } = new("");
        public BindableProperty<string> ActiveDescription { get; } = new("");

        private readonly ChefRegistry _registry;

        public ChefSelectViewModel() : this(null) { }

        public ChefSelectViewModel(ChefRegistry registry)
        {
            _registry = registry;
            SelectChef(ChefId.Rosa);
        }

        public void SelectChef(ChefId chef)
        {
            SelectedChef.Value = chef;
            var def = _registry?.Get(chef);
            ChefName.Value = def?.DisplayName ?? chef.ToString();
            PassiveDescription.Value = def != null ? $"Passive: {def.PassiveAbility}" : "";
            ActiveDescription.Value = def != null ? $"Active: {def.ActiveAbility}" : "";
        }
    }
}
