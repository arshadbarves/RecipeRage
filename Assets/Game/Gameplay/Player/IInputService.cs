using UnityEngine;

namespace RecipeRage
{
    /// <summary>
    /// Dual-stick input. Left stick = move, right stick = aim/interact direction.
    /// Button states are per-frame (true only on the frame pressed).
    /// </summary>
    public interface IInputService
    {
        Vector2 MoveAxis { get; }
        Vector2 AimAxis { get; }
        bool InteractPressed { get; }
        bool ChopPressed { get; }
        void Tick();
    }
}
