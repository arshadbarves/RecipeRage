using Core.GameFramework.Event.Core;
using Core.GameFramework.Scene;

namespace Core.GameFramework.Event.Events.NetworkEvents
{
    public class SceneLoadStartEvent : IGameEvent
    {

        public SceneLoadStartEvent(SceneConfig.GameScene targetScene)
        {
            TargetScene = targetScene;
        }
        public SceneConfig.GameScene TargetScene { get; }
        public bool IsNetworked => true;
    }

    public class SceneLoadCompleteEvent : IGameEvent
    {

        public SceneLoadCompleteEvent(SceneConfig.GameScene loadedScene)
        {
            LoadedScene = loadedScene;
        }
        public SceneConfig.GameScene LoadedScene { get; }
        public bool IsNetworked => true;
    }
}