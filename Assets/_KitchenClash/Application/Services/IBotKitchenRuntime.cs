using System.Collections.Generic;
using UnityEngine;

namespace KitchenClash.Application.Services
{
    public interface IBotKitchenRuntime
    {
        IReadOnlyList<Component> Stations { get; }
    }
}
