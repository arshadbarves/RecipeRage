using System.Collections.Generic;
using UnityEngine;

namespace KitchenClash.Infrastructure.Gameplay
{
    public interface IBotKitchenRuntime
    {
        IReadOnlyList<Component> Stations { get; }
    }
}
