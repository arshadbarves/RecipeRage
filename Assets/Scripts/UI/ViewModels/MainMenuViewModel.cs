using Modules.Shared;
using UI.Core;
using VContainer;

namespace UI.ViewModels
{
    public enum MainMenuTab
    {
        Lobby,
        Character,
        Shop,
        Settings
    }

    public class MainMenuViewModel : BaseViewModel
    {
        public BindableProperty<MainMenuTab> ActiveTab { get; } = new BindableProperty<MainMenuTab>(MainMenuTab.Lobby);
        
        public LobbyViewModel LobbyVM { get; }
        public ShopViewModel ShopVM { get; }
        public SettingsViewModel SettingsVM { get; }
        public CharacterViewModel CharacterVM { get; }

        [Inject]
        public MainMenuViewModel(
            LobbyViewModel lobbyVM,
            ShopViewModel shopVM,
            SettingsViewModel settingsVM,
            CharacterViewModel characterVM)
        {
            LobbyVM = lobbyVM;
            ShopVM = shopVM;
            SettingsVM = settingsVM;
            CharacterVM = characterVM;
        }

        public override void Initialize()
        {
            base.Initialize();
            LobbyVM.Initialize();
            ShopVM.Initialize();
            SettingsVM.Initialize();
            CharacterVM.Initialize();
        }

        public void SwitchTab(MainMenuTab tab)
        {
            ActiveTab.Value = tab;
        }

        public override void Dispose()
        {
            LobbyVM.Dispose();
            ShopVM.Dispose();
            SettingsVM.Dispose();
            CharacterVM.Dispose();
            base.Dispose();
        }
    }
}