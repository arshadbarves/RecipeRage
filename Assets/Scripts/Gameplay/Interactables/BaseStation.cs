using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace RecipeRage.Gameplay.Interactables
{
    /// <summary>
    /// Base class for all kitchen stations with common setup functionality
    /// </summary>
    public abstract class BaseStation : NetworkBehaviour
    {
        #region Enums

        /// <summary>
        /// Visual state of the station material
        /// </summary>
        public enum StationMaterialState
        {
            Normal,
            Highlighted,
            Disabled,
            Processing,
            Success,
            Error
        }

        #endregion

        #region Serialized Fields

        [Header("Base Station Settings")] [SerializeField]
        protected Transform interactionPoint;

        [SerializeField] protected float interactionRadius = 1f;
        [SerializeField] protected LayerMask interactionLayer;

        [Header("Visual Settings")] [SerializeField]
        protected GameObject modelContainer;

        [SerializeField] protected Material stationMaterial;
        [SerializeField] protected ParticleSystem particleEffect;

        [Header("Shader Effect Settings")] [SerializeField]
        protected Color highlightColor = new Color(1f, 0.8f, 0f, 1f);

        [SerializeField] protected float highlightIntensity = 0.5f;
        [SerializeField] protected float highlightPulseSpeed = 2f;
        [SerializeField] protected Color disabledColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        [SerializeField] protected float disabledSaturation = 0.5f;
        [SerializeField] protected Color processingColor = new Color(0f, 0.7f, 1f, 1f);
        [SerializeField] protected float processingPulseSpeed = 3f;
        [SerializeField] protected Color successColor = new Color(0f, 1f, 0.2f, 1f);
        [SerializeField] protected float successDuration = 1f;
        [SerializeField] protected Color errorColor = new Color(1f, 0f, 0f, 1f);
        [SerializeField] protected float errorPulseSpeed = 5f;

        [Header("Mobile Optimization")] [SerializeField]
        protected bool mobileOptimization;

        [SerializeField] [Range(0, 1)] protected float qualityLevel = 1f;

        [Header("Audio Settings")] [SerializeField]
        protected AudioSource audioSource;

        [SerializeField] protected float minPitch = 0.9f;
        [SerializeField] protected float maxPitch = 1.1f;

        // Shader property IDs
        private static readonly int IsHighlightedID = Shader.PropertyToID("_IsHighlighted");
        private static readonly int HighlightColorID = Shader.PropertyToID("_HighlightColor");
        private static readonly int HighlightIntensityID = Shader.PropertyToID("_HighlightIntensity");
        private static readonly int HighlightPulseSpeedID = Shader.PropertyToID("_HighlightPulseSpeed");
        private static readonly int IsDisabledID = Shader.PropertyToID("_IsDisabled");
        private static readonly int DisabledColorID = Shader.PropertyToID("_DisabledColor");
        private static readonly int DisabledSaturationID = Shader.PropertyToID("_DisabledSaturation");
        private static readonly int IsProcessingID = Shader.PropertyToID("_IsProcessing");
        private static readonly int ProcessingColorID = Shader.PropertyToID("_ProcessingColor");
        private static readonly int ProcessingPulseSpeedID = Shader.PropertyToID("_ProcessingPulseSpeed");
        private static readonly int IsSuccessID = Shader.PropertyToID("_IsSuccess");
        private static readonly int SuccessColorID = Shader.PropertyToID("_SuccessColor");
        private static readonly int SuccessDurationID = Shader.PropertyToID("_SuccessDuration");
        private static readonly int IsErrorID = Shader.PropertyToID("_IsError");
        private static readonly int ErrorColorID = Shader.PropertyToID("_ErrorColor");
        private static readonly int ErrorPulseSpeedID = Shader.PropertyToID("_ErrorPulseSpeed");
        private static readonly int MobileModeID = Shader.PropertyToID("_MobileMode");
        private static readonly int QualityLevelID = Shader.PropertyToID("_QualityLevel");

        private Material[] instancedMaterials;

        #endregion

        #region Unity Lifecycle

        protected virtual void OnValidate()
        {
            // Auto-setup if components are missing
            if (interactionPoint == null)
            {
                var point = new GameObject("InteractionPoint");
                point.transform.SetParent(transform);
                point.transform.localPosition = Vector3.up;
                interactionPoint = point.transform;
            }

            if (audioSource == null)
            {
                audioSource = gameObject.GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                    audioSource.spatialBlend = 1f; // Full 3D
                    audioSource.rolloffMode = AudioRolloffMode.Linear;
                    audioSource.minDistance = 1f;
                    audioSource.maxDistance = 10f;
                }
            }

            // Setup material instances if needed
            if (modelContainer != null && stationMaterial != null)
            {
                SetupMaterialInstances();
                UpdateShaderProperties();
            }
        }

        protected virtual void Awake()
        {
            SetupComponents();
            if (modelContainer != null && stationMaterial != null)
            {
                SetupMaterialInstances();
                UpdateShaderProperties();
            }

            // Auto-detect mobile platform and set optimization
            AutoDetectMobilePlatform();
        }

        protected virtual void OnDrawGizmos()
        {
            if (interactionPoint != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(interactionPoint.position, interactionRadius);
            }
        }

        #endregion

        #region Protected Methods

        protected void PlaySound(AudioClip clip, float volumeMultiplier = 1f)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.pitch = Random.Range(minPitch, maxPitch);
                audioSource.PlayOneShot(clip, volumeMultiplier);
            }
        }

        protected void SetHighlight(bool active)
        {
            if (instancedMaterials != null)
                foreach (var material in instancedMaterials)
                    if (material != null)
                        material.SetFloat(IsHighlightedID, active ? 1f : 0f);
        }

        protected void SetDisabled(bool disabled)
        {
            if (instancedMaterials != null)
                foreach (var material in instancedMaterials)
                    if (material != null)
                        material.SetFloat(IsDisabledID, disabled ? 1f : 0f);
        }

        protected void PlayParticles(bool play = true)
        {
            if (particleEffect != null)
            {
                if (play)
                    particleEffect.Play();
                else
                    particleEffect.Stop();
            }
        }

        /// <summary>
        /// Sets the station's material to active or inactive state.
        /// </summary>
        /// <param name="active"> True to set the station to active state, false for inactive state </param>
        /// <remarks>
        /// This method controls the visual appearance of the station to indicate whether
        /// it's in a usable/active state. It uses the shader's disabled property to create
        /// the visual effect.
        /// </remarks>
        protected void SetMaterial(bool active)
        {
            if (instancedMaterials == null || instancedMaterials.Length == 0) return;

            SetDisabled(!active);
        }

        /// <summary>
        /// Sets the station's material to a specific visual state.
        /// </summary>
        /// <param name="state"> The visual state to apply to the station </param>
        /// <remarks>
        /// This enhanced method provides more control over the station's visual appearance
        /// by supporting multiple states like active, processing, success, and error states.
        /// </remarks>
        protected void SetStationMaterial(StationMaterialState state)
        {
            if (instancedMaterials == null || instancedMaterials.Length == 0) return;

            // Reset all states first
            ResetAllStates();

            // Apply the requested state
            switch (state)
            {
                case StationMaterialState.Normal:
                    // Normal state has all effects disabled
                    break;

                case StationMaterialState.Highlighted:
                    SetHighlight(true);
                    break;

                case StationMaterialState.Disabled:
                    SetDisabled(true);
                    break;

                case StationMaterialState.Processing:
                    SetProcessing(true);
                    break;

                case StationMaterialState.Success:
                    SetSuccess(true);

                    // Auto-reset success state after duration
                    if (IsClient) StartCoroutine(AutoResetState(successDuration));
                    break;

                case StationMaterialState.Error:
                    SetError(true);

                    // Auto-reset error state after duration
                    if (IsClient) StartCoroutine(AutoResetState(2f));
                    break;
            }
        }

        /// <summary>
        /// Automatically resets the station material state after a delay
        /// </summary>
        private IEnumerator AutoResetState(float delay)
        {
            yield return new WaitForSeconds(delay);
            ResetAllStates();
        }

        /// <summary>
        /// Sets the processing visual state of the station.
        /// </summary>
        /// <param name="isProcessing"> Whether the station is in processing state </param>
        protected void SetProcessing(bool isProcessing)
        {
            if (instancedMaterials != null)
                foreach (var material in instancedMaterials)
                    if (material != null)
                        material.SetFloat(IsProcessingID, isProcessing ? 1f : 0f);
        }

        /// <summary>
        /// Sets the success visual state of the station.
        /// </summary>
        /// <param name="isSuccess"> Whether the station is in success state </param>
        protected void SetSuccess(bool isSuccess)
        {
            if (instancedMaterials != null)
                foreach (var material in instancedMaterials)
                    if (material != null)
                        material.SetFloat(IsSuccessID, isSuccess ? 1f : 0f);
        }

        /// <summary>
        /// Sets the error visual state of the station.
        /// </summary>
        /// <param name="isError"> Whether the station is in error state </param>
        protected void SetError(bool isError)
        {
            if (instancedMaterials != null)
                foreach (var material in instancedMaterials)
                    if (material != null)
                        material.SetFloat(IsErrorID, isError ? 1f : 0f);
        }

        /// <summary>
        /// Resets all visual states to their default values.
        /// </summary>
        protected void ResetAllStates()
        {
            if (instancedMaterials != null)
                foreach (var material in instancedMaterials)
                    if (material != null)
                    {
                        material.SetFloat(IsHighlightedID, 0f);
                        material.SetFloat(IsDisabledID, 0f);
                        material.SetFloat(IsProcessingID, 0f);
                        material.SetFloat(IsSuccessID, 0f);
                        material.SetFloat(IsErrorID, 0f);
                    }
        }

        /// <summary>
        /// Sets the mobile optimization mode for the station materials.
        /// </summary>
        /// <param name="enableMobileMode"> Whether to enable mobile optimization </param>
        /// <param name="quality"> Quality level between 0 and 1 </param>
        protected void SetMobileOptimization(bool enableMobileMode, float quality = 1f)
        {
            if (instancedMaterials != null)
                foreach (var material in instancedMaterials)
                    if (material != null)
                    {
                        material.SetFloat(MobileModeID, enableMobileMode ? 1f : 0f);
                        material.SetFloat(QualityLevelID, Mathf.Clamp01(quality));
                    }
        }

        #endregion

        #region Private Methods

        private void SetupComponents()
        {
            // Ensure we have a NetworkObject
            if (GetComponent<NetworkObject>() == null) gameObject.AddComponent<NetworkObject>();

            // Setup audio source defaults if needed
            if (audioSource != null)
            {
                audioSource.spatialBlend = 1f;
                audioSource.rolloffMode = AudioRolloffMode.Linear;
                audioSource.minDistance = 1f;
                audioSource.maxDistance = 10f;
            }

            // Setup collider if needed
            var collider = GetComponent<Collider>();
            if (collider == null)
            {
                var boxCollider = gameObject.AddComponent<BoxCollider>();
                boxCollider.size = new Vector3(1f, 1f, 1f);
                boxCollider.center = new Vector3(0f, 0.5f, 0f);
            }
        }

        private void SetupMaterialInstances()
        {
            Renderer[] renderers = modelContainer.GetComponentsInChildren<Renderer>();
            instancedMaterials = new Material[renderers.Length];

            for (int i = 0; i < renderers.Length; i++)
            {
                // Create instance of the material for each renderer
                instancedMaterials[i] = new Material(stationMaterial);
                renderers[i].material = instancedMaterials[i];
            }
        }

        private void UpdateShaderProperties()
        {
            if (instancedMaterials == null || instancedMaterials.Length == 0)
                return;

            // Use MaterialPropertyBlock for more efficient property updates
            var propertyBlock = new MaterialPropertyBlock();

            // Set all properties in the block
            propertyBlock.SetColor(HighlightColorID, highlightColor);
            propertyBlock.SetFloat(HighlightIntensityID, highlightIntensity);
            propertyBlock.SetFloat(HighlightPulseSpeedID, highlightPulseSpeed);
            propertyBlock.SetColor(DisabledColorID, disabledColor);
            propertyBlock.SetFloat(DisabledSaturationID, disabledSaturation);

            // Advanced state properties
            propertyBlock.SetColor(ProcessingColorID, processingColor);
            propertyBlock.SetFloat(ProcessingPulseSpeedID, processingPulseSpeed);
            propertyBlock.SetColor(SuccessColorID, successColor);
            propertyBlock.SetFloat(SuccessDurationID, successDuration);
            propertyBlock.SetColor(ErrorColorID, errorColor);
            propertyBlock.SetFloat(ErrorPulseSpeedID, errorPulseSpeed);

            // Mobile optimization
            propertyBlock.SetFloat(MobileModeID, mobileOptimization ? 1f : 0f);
            propertyBlock.SetFloat(QualityLevelID, qualityLevel);

            // Find all renderers and apply the property block
            Renderer[] renderers = modelContainer.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
                if (renderer != null)
                    renderer.SetPropertyBlock(propertyBlock);

            // Also update the instanced materials for compatibility
            foreach (var material in instancedMaterials)
                if (material != null)
                {
                    // Basic properties
                    material.SetColor(HighlightColorID, highlightColor);
                    material.SetFloat(HighlightIntensityID, highlightIntensity);
                    material.SetFloat(HighlightPulseSpeedID, highlightPulseSpeed);
                    material.SetColor(DisabledColorID, disabledColor);
                    material.SetFloat(DisabledSaturationID, disabledSaturation);

                    // Advanced state properties
                    material.SetColor(ProcessingColorID, processingColor);
                    material.SetFloat(ProcessingPulseSpeedID, processingPulseSpeed);
                    material.SetColor(SuccessColorID, successColor);
                    material.SetFloat(SuccessDurationID, successDuration);
                    material.SetColor(ErrorColorID, errorColor);
                    material.SetFloat(ErrorPulseSpeedID, errorPulseSpeed);

                    // Mobile optimization
                    material.SetFloat(MobileModeID, mobileOptimization ? 1f : 0f);
                    material.SetFloat(QualityLevelID, qualityLevel);
                }
        }

        private void OnDestroy()
        {
            // Clean up instanced materials
            if (instancedMaterials != null)
                foreach (var material in instancedMaterials)
                    if (material != null)
                        Destroy(material);
        }

        /// <summary>
        /// Automatically detects if running on a mobile platform and sets optimization accordingly
        /// </summary>
        private void AutoDetectMobilePlatform()
        {
#if UNITY_ANDROID || UNITY_IOS
            // Automatically enable mobile optimization on mobile platforms
            mobileOptimization = true;
            
            // Set quality level based on device performance
            // You can adjust this based on device specs or quality settings
            qualityLevel = Mathf.Clamp01(QualitySettings.GetQualityLevel() / 5f);
            
            // Apply settings to materials
            if (instancedMaterials != null)
            {
                SetMobileOptimization(mobileOptimization, qualityLevel);
            }
#endif
        }

        #endregion
    }
}