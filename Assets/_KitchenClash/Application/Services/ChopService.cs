using KitchenClash.Domain;

namespace KitchenClash.Application.Services
{
    public class ChopService
    {
        private readonly IConfigService _cfg;

        public ChopService(IConfigService cfg) => _cfg = cfg;

        public int GetTapsRequired(IngredientType ingredient)
        {
            return ingredient switch
            {
                IngredientType.Lettuce => _cfg.Get(ChopConfig.ChopTapsLettuce, ChopConfig.DefaultLettuce),
                IngredientType.Fish => _cfg.Get(ChopConfig.ChopTapsFish, ChopConfig.DefaultFish),
                IngredientType.Beef => _cfg.Get(ChopConfig.ChopTapsMeat, ChopConfig.DefaultMeat),
                IngredientType.Chicken => _cfg.Get(ChopConfig.ChopTapsMeat, ChopConfig.DefaultMeat),
                IngredientType.Vegetables => _cfg.Get(ChopConfig.ChopTapsCarrot, ChopConfig.DefaultCarrot),
                _ => 3
            };
        }

        public int GetTapCapPerSecond() => _cfg.Get(ChopConfig.ChopTapCapPerSec, ChopConfig.DefaultCapPerSec);
    }
}
