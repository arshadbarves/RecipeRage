using KitchenClash.Application;
using KitchenClash.Infrastructure.Network;
using UnityEngine;
using VContainer;
using VContainer.Unity;

/// <summary>
/// MainMenu scene bind-in for presentation ports owned at Root.
/// Mirrors match runtime scene binding: scene MonoBehaviours never install session
/// services; they only attach to Root gateways after the scene loads.
/// </summary>
public sealed class MenuSceneBinder : MonoBehaviour
{
    [SerializeField] private CharacterPreviewManager _previewManager;

    private CharacterPreviewGateway _gateway;
    private ICharacterPreviewService _boundPreview;

    private void Start()
    {
        LifetimeScope root = LifetimeScope.Find<RootLifetimeScope>();
        if (root == null || root.Container == null)
        {
            Debug.LogWarning("[MenuSceneBinder] RootLifetimeScope not found; preview bind skipped.");
            return;
        }

        _gateway = root.Container.Resolve<CharacterPreviewGateway>();
        CharacterPreviewManager preview = _previewManager != null
            ? _previewManager
            : Object.FindFirstObjectByType<CharacterPreviewManager>();

        if (preview == null || _gateway == null)
        {
            return;
        }

        _gateway.Bind(preview);
        _boundPreview = preview;
        _previewManager = preview;
    }

    private void OnDestroy()
    {
        if (_gateway != null && _boundPreview != null)
        {
            _gateway.Unbind(_boundPreview);
            _boundPreview = null;
        }
    }
}
