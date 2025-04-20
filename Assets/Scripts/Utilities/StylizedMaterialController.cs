using UnityEngine;

namespace RecipeRage.Utilities
{
    /// <summary>
    /// Controls the stylized material properties for an object.
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(Renderer))]
    public class StylizedMaterialController : MonoBehaviour
    {
        [Header("Material References")]
        [SerializeField] private Material _stylizedMaterial;
        [SerializeField] private bool _createMaterialInstance = true;
        [SerializeField] private bool _applyToChildren = false;
        
        [Header("Base Properties")]
        [SerializeField] private Color _color = Color.white;
        
        [Header("Ambient Lighting")]
        [SerializeField] private Color _ambientColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        [SerializeField, Range(0f, 1f)] private float _ambientIntensity = 0.5f;
        
        [Header("Fake Directional Light")]
        [SerializeField] private Vector3 _lightDirection = new Vector3(0.5f, 0.5f, 0f);
        [SerializeField] private Color _lightColor = Color.white;
        [SerializeField, Range(0f, 2f)] private float _lightIntensity = 1f;
        [SerializeField, Range(0f, 1f)] private float _shadowSoftness = 0.5f;
        
        [Header("Specular")]
        [SerializeField] private Color _specularColor = Color.white;
        [SerializeField, Range(1f, 256f)] private float _glossiness = 32f;
        [SerializeField, Range(0f, 2f)] private float _specularIntensity = 1f;
        
        [Header("Material Properties")]
        [SerializeField, Range(0f, 1f)] private float _metallic = 0f;
        [SerializeField, Range(0f, 1f)] private float _roughness = 0.5f;
        
        [Header("Rim Effect")]
        [SerializeField] private Color _rimColor = Color.white;
        [SerializeField, Range(0.5f, 8f)] private float _rimPower = 3f;
        [SerializeField, Range(0f, 1f)] private float _rimIntensity = 0.5f;
        
        [Header("Update Settings")]
        [SerializeField] private bool _updateInPlayMode = false;
        [SerializeField] private bool _updateInEditMode = true;
        
        private Renderer _renderer;
        private Material _materialInstance;
        private Renderer[] _childRenderers;
        
        private void OnEnable()
        {
            _renderer = GetComponent<Renderer>();
            
            if (_applyToChildren)
            {
                _childRenderers = GetComponentsInChildren<Renderer>(true);
            }
            
            if (_createMaterialInstance && _stylizedMaterial != null)
            {
                CreateMaterialInstance();
            }
            
            UpdateMaterialProperties();
        }
        
        private void Update()
        {
            if (Application.isPlaying && !_updateInPlayMode) return;
            if (!Application.isPlaying && !_updateInEditMode) return;
            
            UpdateMaterialProperties();
        }
        
        /// <summary>
        /// Creates a material instance from the stylized material.
        /// </summary>
        public void CreateMaterialInstance()
        {
            if (_stylizedMaterial == null)
            {
                Debug.LogError("Stylized material is not assigned!");
                return;
            }
            
            _materialInstance = new Material(_stylizedMaterial);
            _materialInstance.name = $"{_stylizedMaterial.name} (Instance)";
            
            _renderer.material = _materialInstance;
            
            if (_applyToChildren && _childRenderers != null)
            {
                foreach (var childRenderer in _childRenderers)
                {
                    if (childRenderer != _renderer) // Skip the parent renderer
                    {
                        childRenderer.material = _materialInstance;
                    }
                }
            }
        }
        
        /// <summary>
        /// Updates the material properties based on the current settings.
        /// </summary>
        public void UpdateMaterialProperties()
        {
            if (_materialInstance == null && _createMaterialInstance)
            {
                CreateMaterialInstance();
            }
            
            Material targetMaterial = _materialInstance != null ? _materialInstance : _renderer.material;
            
            // Update base properties
            targetMaterial.SetColor("_Color", _color);
            
            // Update ambient lighting properties
            targetMaterial.SetColor("_AmbientColor", _ambientColor);
            targetMaterial.SetFloat("_AmbientIntensity", _ambientIntensity);
            
            // Update fake directional light properties
            targetMaterial.SetVector("_LightDirection", new Vector4(_lightDirection.x, _lightDirection.y, _lightDirection.z, 0f));
            targetMaterial.SetColor("_LightColor", _lightColor);
            targetMaterial.SetFloat("_LightIntensity", _lightIntensity);
            
            // Check if the shader supports shadow softness (full version only)
            if (targetMaterial.HasProperty("_ShadowSoftness"))
            {
                targetMaterial.SetFloat("_ShadowSoftness", _shadowSoftness);
            }
            
            // Update specular properties
            targetMaterial.SetColor("_SpecularColor", _specularColor);
            targetMaterial.SetFloat("_Glossiness", _glossiness);
            targetMaterial.SetFloat("_SpecularIntensity", _specularIntensity);
            
            // Check if the shader supports metallic and roughness (full version only)
            if (targetMaterial.HasProperty("_Metallic"))
            {
                targetMaterial.SetFloat("_Metallic", _metallic);
            }
            
            if (targetMaterial.HasProperty("_Roughness"))
            {
                targetMaterial.SetFloat("_Roughness", _roughness);
            }
            
            // Update rim effect properties
            targetMaterial.SetColor("_RimColor", _rimColor);
            targetMaterial.SetFloat("_RimPower", _rimPower);
            
            // Check if the shader supports rim intensity (full version only)
            if (targetMaterial.HasProperty("_RimIntensity"))
            {
                targetMaterial.SetFloat("_RimIntensity", _rimIntensity);
            }
        }
        
        /// <summary>
        /// Sets the main color of the material.
        /// </summary>
        public void SetColor(Color color)
        {
            _color = color;
            UpdateMaterialProperties();
        }
        
        /// <summary>
        /// Sets the light direction.
        /// </summary>
        public void SetLightDirection(Vector3 direction)
        {
            _lightDirection = direction.normalized;
            UpdateMaterialProperties();
        }
        
        /// <summary>
        /// Sets the metallic and roughness values.
        /// </summary>
        public void SetMetallicRoughness(float metallic, float roughness)
        {
            _metallic = Mathf.Clamp01(metallic);
            _roughness = Mathf.Clamp01(roughness);
            UpdateMaterialProperties();
        }
        
        /// <summary>
        /// Sets the rim effect properties.
        /// </summary>
        public void SetRimEffect(Color color, float power, float intensity)
        {
            _rimColor = color;
            _rimPower = Mathf.Clamp(power, 0.5f, 8f);
            _rimIntensity = Mathf.Clamp01(intensity);
            UpdateMaterialProperties();
        }
        
        /// <summary>
        /// Sets the specular properties.
        /// </summary>
        public void SetSpecular(Color color, float glossiness, float intensity)
        {
            _specularColor = color;
            _glossiness = Mathf.Clamp(glossiness, 1f, 256f);
            _specularIntensity = Mathf.Clamp(intensity, 0f, 2f);
            UpdateMaterialProperties();
        }
    }
}
