using KitchenClash.Domain;
using UnityEngine;
using VContainer.Unity;

namespace KitchenClash.Presentation
{
    /// <summary>
    /// GDD: BackButtonHandler (IStartable, pure C#).
    /// Handles Android back button.
    /// </summary>
    public sealed class BackButtonHandler : IStartable, ITickable
    {
        private readonly IRouterService _router;

        public BackButtonHandler(IRouterService router)
        {
            _router = router;
        }

        void IStartable.Start() { }

        void ITickable.Tick()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _router.Pop();
            }
        }
    }
}
