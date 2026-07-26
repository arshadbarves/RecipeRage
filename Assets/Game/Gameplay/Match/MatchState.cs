using System.Collections.Generic;

namespace RecipeRage
{
    public sealed class MatchState
    {
        public List<RecipeDefinition> RecipeList;
        public int CurrentIndex;
        public float RemainingSeconds;
        public bool IsOver;
    }
}
