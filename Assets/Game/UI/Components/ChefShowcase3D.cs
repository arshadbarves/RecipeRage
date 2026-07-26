using Playcenter;
using UnityEngine;
using UnityEngine.UIElements;

namespace RecipeRage.UI
{
    /// <summary>
    /// Renders the selected chef's 3D model into a RenderTexture shown in UI.
    /// Drag to rotate; swipe left/right cycles unlocked chefs.
    /// </summary>
    public sealed class ChefShowcase3D : MonoBehaviour
    {
        [SerializeField] private RenderTexture _renderTexture;
        [SerializeField] private Transform _modelAnchor;
        [SerializeField] private float _rotateSpeed = 60f;

        private GameObject _currentModel;
        private VisualElement _boundElement;
        private IChefProgressionService _progression;
        private IChefCatalog _catalog;
        private bool _idleRotate = true;

        public void Bind(VisualElement element)
        {
            _boundElement = element;
            _boundElement.style.backgroundImage = new StyleBackground(Background.FromRenderTexture(_renderTexture));
            _progression = ServiceLocator.Get<IChefProgressionService>();
            _catalog = ServiceLocator.Get<IChefCatalog>();

            _boundElement.RegisterCallback<PointerMoveEvent>(OnDrag);
            _boundElement.RegisterCallback<PointerDownEvent>(e => _idleRotate = false);
            _boundElement.RegisterCallback<PointerUpEvent>(e => _idleRotate = true);

            _progression.OnChefSelected += OnChefSelected;
            ShowChef(_progression.GetSelectedChef());
        }

        private void OnChefSelected(ChefId id) => ShowChef(id);

        private void ShowChef(ChefId id)
        {
            if (_currentModel != null)
            {
                Destroy(_currentModel);
            }

            var chef = _catalog.Get(id);
            if (chef != null && chef.ModelPrefab != null)
            {
                _currentModel = Instantiate(chef.ModelPrefab, _modelAnchor);
                _currentModel.transform.localPosition = Vector3.zero;
            }
        }

        private void Update()
        {
            if (_currentModel != null && _idleRotate)
            {
                _currentModel.transform.Rotate(Vector3.up, _rotateSpeed * Time.deltaTime * 0.3f);
            }
        }

        private void OnDrag(PointerMoveEvent e)
        {
            if (_currentModel != null && e.pressedButtons == 1)
            {
                _currentModel.transform.Rotate(Vector3.up, -e.deltaPosition.x * 0.5f);
            }
        }
    }
}
