using System;

namespace KitchenClash.Application.Input
{
    /// <summary>
    /// Cross-assembly bridge for gameplay action buttons (mobile UI → player input).
    /// Presentation raises; Infrastructure input providers / PlayerController subscribe.
    /// Avoids Infrastructure → Presentation references.
    /// </summary>
    public static class GameplayInputBridge
    {
        public static event Action AttackPressed;
        public static event Action InteractPressed;
        public static event Action SpecialPressed;

        public static void RaiseAttack() => AttackPressed?.Invoke();
        public static void RaiseInteract() => InteractPressed?.Invoke();
        public static void RaiseSpecial() => SpecialPressed?.Invoke();
    }
}
