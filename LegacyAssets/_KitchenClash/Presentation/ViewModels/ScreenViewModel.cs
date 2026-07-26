using KitchenClash.Domain;

namespace KitchenClash.Presentation.ViewModels
{
    public abstract class ScreenViewModel
    {
        public virtual void OnEnter(object param) { }
        public virtual void OnExit() { }
        public virtual void OnResume() { }
    }
}
