using KitchenClash.Domain;

namespace KitchenClash.Presentation.ViewModels
{
    public sealed class ChefSelectViewModel : ScreenViewModel
    {
        public BindableProperty<ChefId> SelectedChef { get; } = new(ChefId.Rosa);
        public BindableProperty<string> ChefName { get; } = new("Rosa");
        public BindableProperty<string> PassiveDescription { get; } = new("");
        public BindableProperty<string> ActiveDescription { get; } = new("");

        public void SelectChef(ChefId chef)
        {
            SelectedChef.Value = chef;
            ChefName.Value = chef.ToString();
        }
    }
}
