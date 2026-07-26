using System.Collections.Generic;
using UnityEngine;

namespace RecipeRage
{
    public interface IRecipeCatalog
    {
        /// <summary>
        /// Builds a shuffled match list: same list generated from the same seed
        /// (server generates in Slice 2, both teams receive identical lists).
        /// </summary>
        List<RecipeDefinition> GetRandomRecipeList(int easy, int medium, int hard, int seed);
    }

    public sealed class RecipeCatalog : IRecipeCatalog
    {
        private readonly List<RecipeDefinition> _easy = new List<RecipeDefinition>();
        private readonly List<RecipeDefinition> _medium = new List<RecipeDefinition>();
        private readonly List<RecipeDefinition> _hard = new List<RecipeDefinition>();

        public RecipeCatalog(RecipeDefinition[] allRecipes)
        {
            foreach (var recipe in allRecipes)
            {
                switch (recipe.Tier)
                {
                    case RecipeTier.Easy: _easy.Add(recipe); break;
                    case RecipeTier.Medium: _medium.Add(recipe); break;
                    case RecipeTier.Hard: _hard.Add(recipe); break;
                }
            }
        }

        public List<RecipeDefinition> GetRandomRecipeList(int easy, int medium, int hard, int seed)
        {
            var rng = new System.Random(seed);
            var result = new List<RecipeDefinition>(easy + medium + hard);
            PickRandom(_easy, easy, rng, result);
            PickRandom(_medium, medium, rng, result);
            PickRandom(_hard, hard, rng, result);
            return result;
        }

        private static void PickRandom(List<RecipeDefinition> source, int count, System.Random rng, List<RecipeDefinition> result)
        {
            var pool = new List<RecipeDefinition>(source);
            for (int i = 0; i < count && pool.Count > 0; i++)
            {
                int index = rng.Next(pool.Count);
                result.Add(pool[index]);
                pool.RemoveAt(index);
            }
        }
    }
}
