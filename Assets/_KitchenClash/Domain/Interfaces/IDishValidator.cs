using System.Collections;

namespace KitchenClash.Domain
{
    public interface IDishValidator
    {
        bool ValidateDish(IList ingredients, object recipe);
        DishQuality GetDishQuality(IList ingredients, object recipe);
        int CalculateScore(IList ingredients, object recipe, float timeRemaining);
    }
}
