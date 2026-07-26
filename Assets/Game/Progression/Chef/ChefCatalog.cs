using System.Collections.Generic;

namespace RecipeRage
{
    public interface IChefCatalog
    {
        IReadOnlyList<ChefDefinition> All { get; }
        ChefDefinition Get(ChefId id);
        ChefAbilityModifier BuildModifier(ChefId id, int level);
    }

    public sealed class ChefCatalog : IChefCatalog
    {
        private readonly Dictionary<ChefId, ChefDefinition> _byId = new Dictionary<ChefId, ChefDefinition>(8);

        public IReadOnlyList<ChefDefinition> All { get; }

        public ChefCatalog(ChefDefinition[] chefs)
        {
            All = chefs;
            foreach (var chef in chefs)
            {
                _byId[chef.Id] = chef;
            }
        }

        public ChefDefinition Get(ChefId id) => _byId.TryGetValue(id, out var chef) ? chef : null;

        public ChefAbilityModifier BuildModifier(ChefId id, int level)
        {
            var chef = Get(id);
            if (chef == null)
            {
                return ChefAbilityModifier.None;
            }

            var index = Mathf_Clamp(level - 1, 0, chef.AbilityPerLevel.Length - 1);
            var value = chef.AbilityPerLevel[index];

            return chef.AbilityType switch
            {
                ChefAbilityType.MoveSpeed => new ChefAbilityModifier(1f + value, 1f, 0, false, 0f),
                ChefAbilityType.PickupDropSpeed => new ChefAbilityModifier(1f, 1f + value, 0, false, 0f),
                ChefAbilityType.CarryCapacity => new ChefAbilityModifier(1f, 1f, (int)value, false, 0f),
                ChefAbilityType.Dash => new ChefAbilityModifier(1f, 1f, 0, true, value),
                _ => ChefAbilityModifier.None,
            };
        }

        private static int Mathf_Clamp(int value, int min, int max)
        {
            return value < min ? min : value > max ? max : value;
        }
    }
}
