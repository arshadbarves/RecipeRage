using UnityEngine;
using UnityEngine.UIElements;
using KitchenClash.Application;
using KitchenClash.Domain;

namespace KitchenClash.Presentation
{
    /// <summary>
    /// GDD Section 4: Base class binding UXML to ViewModel.
    /// </summary>
    public abstract class ScreenPresenter<TVM> : IScreen where TVM : ScreenViewModel
    {
        protected readonly TVM ViewModel;
        protected VisualElement Root { get; private set; }

        protected abstract ScreenId ScreenId { get; }

        protected ScreenPresenter(TVM viewModel)
        {
            ViewModel = viewModel;
        }

        public void Enter(VisualElement parent, TransitionDir dir, object param)
        {
            var asset = Resources.Load<VisualTreeAsset>($"UI/{ScreenId}");
            Root = asset.CloneTree();
            Root.AddToClassList("screen");
            Root.AddToClassList(dir == TransitionDir.Right
                ? "screen--enter-right" : "screen--enter-left");
            parent.Add(Root);

            BindViewModel();
            ViewModel.OnEnter(param);

            // One-frame trick to trigger CSS transition (GDD)
            Root.schedule.Execute(() =>
            {
                Root.RemoveFromClassList("screen--enter-right");
                Root.RemoveFromClassList("screen--enter-left");
                Root.RemoveFromClassList("screen--enter-bottom");
            }).StartingIn(16);
        }

        public void Exit(TransitionDir dir)
        {
            Root?.AddToClassList(dir == TransitionDir.Left
                ? "screen--exit-left" : "screen--exit-right");

            ViewModel.OnExit();

            // Remove after transition completes
            Root?.schedule.Execute(() => Root?.RemoveFromHierarchy()).StartingIn(300);
        }

        public void Resume(TransitionDir dir)
        {
            Root?.RemoveFromClassList("screen--exit-left");
            Root?.RemoveFromClassList("screen--exit-right");
            ViewModel.OnResume();
        }

        protected abstract void BindViewModel();
    }
}
