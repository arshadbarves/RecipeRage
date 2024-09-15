using Core;

namespace GameSystem.State.GameStates
{
    public class CharacterSelectionState: BaseGameState
    {
        public CharacterSelectionState(GameManager gameManager) : base(gameManager)
        {
        }

        public override void Enter()
        {
            // GameManager.GetSystem<UISystem>().ShowCharacterSelection();
        }

        public override void Update()
        {
            
        }

        public override void Exit()
        {
            // GameManager.GetSystem<UISystem>().HideCharacterSelection();
        }
    }
}