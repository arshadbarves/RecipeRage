using System;

namespace Playcenter.MobileCore
{
    [Flags]
    public enum InputButtons : byte
    {
        None = 0,
        Interact = 1 << 0,
        Ability = 1 << 1,
        Super = 1 << 2,
        Gadget = 1 << 3,
        ChopTap = 1 << 4,
        AimReleased = 1 << 5,
    }
}
