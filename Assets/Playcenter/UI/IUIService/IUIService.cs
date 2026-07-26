using System;

namespace Playcenter.UI
{
    public interface IUIService
    {
        event Action<BaseUIScreen> OnScreenShown;
        BaseUIScreen Current { get; }
        void Register(BaseUIScreen screen);
        void Show<T>() where T : BaseUIScreen;
        void Hide<T>() where T : BaseUIScreen;
        void HideAll();
    }
}
