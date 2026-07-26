using Playcenter;
using Playcenter.Services;

namespace RecipeRage
{
    /// <summary>
    /// Maps gameplay events to SFX. Lives in the game assembly (knows both sides) —
    /// Playcenter.Services must not reference game types.
    /// </summary>
    public sealed class GameplayAudioWiring
    {
        public void Initialize(IEventBus bus, IAudioService audio)
        {
            bus.Subscribe<IngredientChoppedEvent>(e => audio.Play(SfxId.KnifeChop));
            bus.Subscribe<CookingCompletedEvent>(e => audio.Play(SfxId.CookingDone));
            bus.Subscribe<IngredientBurntEvent>(e => audio.Play(SfxId.Burning));
            bus.Subscribe<RecipeServedEvent>(e => audio.Play(SfxId.RecipeComplete));
            bus.Subscribe<IngredientFetchedEvent>(e => audio.Play(SfxId.Pickup));
            bus.Subscribe<IngredientPlatedEvent>(e => audio.Play(SfxId.PlateArrange));
            bus.Subscribe<PlateTakenEvent>(e => audio.Play(SfxId.Pickup));
            bus.Subscribe<MatchEndedEvent>(e => audio.Play(e.Won ? SfxId.Victory : SfxId.Defeat));
        }
    }
}
