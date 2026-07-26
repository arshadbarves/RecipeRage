using UnityEngine;

namespace RecipeRage
{
    [CreateAssetMenu(fileName = "Map", menuName = "RecipeRage/Map Definition")]
    public sealed class MapDefinition : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField] private string _sceneName;

        public string Id => _id;
        public string DisplayName => _displayName;
        public string SceneName => _sceneName;
    }
}
