using KitchenClash.Domain;

namespace KitchenClash.Application
{
    public abstract class ScreenViewModel
    {
        public virtual void OnEnter(object param) { }
        public virtual void OnExit() { }
        public virtual void OnResume() { }
    }
}
