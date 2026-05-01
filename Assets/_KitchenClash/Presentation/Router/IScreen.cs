using UnityEngine.UIElements;

namespace KitchenClash.Presentation
{
    public enum TransitionDir
    {
        Left,
        Right,
        Bottom
    }

    public interface IScreen
    {
        void Enter(VisualElement parent, TransitionDir dir, object param);
        void Exit(TransitionDir dir);
        void Resume(TransitionDir dir);
    }
}
