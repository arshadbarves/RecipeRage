using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Shared
{
    /// <summary>
    /// Read-only runtime view of bot-relevant kitchen stations for the active match.
    /// </summary>
    public interface IBotKitchenRuntime
    {
        IReadOnlyList<Component> Stations { get; }
    }
}
