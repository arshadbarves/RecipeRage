using UnityEngine;

namespace RecipeRage.Utilities
{
    /// <summary>
    /// Utility script to apply the Fall Guys style shader to objects
    /// </summary>
    [ExecuteInEditMode]
    public class FallGuysStyleApplier : MonoBehaviour
    {
        [Header("Shader Settings")]
        [SerializeField] private Material fallGuysStyleMaterial;
        [SerializeField] private Color mainColor = Color.white;
        [SerializeField] private Color ambientColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        [SerializeField] [Range(0f, 1f)] private float ambientIntensity = 0.5f;
        [SerializeField] private Color rimColor = Color.white;
        [SerializeField] [Range(0.5f, 8f)] private float rimPower = 3f;
        [SerializeField] [Range(0f, 1f)] private float softFactor = 0.5f;
        
        [Header("Application Settings")]
        [SerializeField] private bool applyToChildren = true;
        [SerializeField] private bool applyOnStart = true;
        
        private void Start()
        {
            if (applyOnStart)
            {
                ApplyStyle();
            }
        }
        
        /// <summary>
        /// Apply the Fall Guys style to this object and optionally its children
        /// </summary>
        public void ApplyStyle()
        {
            if (fallGuysStyleMaterial == null)
            {
                Debug.LogError("Fall Guys Style Material is not assigned!");
                return;
            }
            
            // Create a new instance of the material
            Material instanceMaterial = new Material(fallGuysStyleMaterial);
            
            // Set the properties
            instanceMaterial.SetColor("_Color", mainColor);
            instanceMaterial.SetColor("_AmbientColor", ambientColor);
            instanceMaterial.SetFloat("_AmbientIntensity", ambientIntensity);
            instanceMaterial.SetColor("_RimColor", rimColor);
            instanceMaterial.SetFloat("_RimPower", rimPower);
            
            // Try to set the soft factor (it might not exist in the simplified shader)
            if (instanceMaterial.HasProperty("_SoftFactor"))
            {
                instanceMaterial.SetFloat("_SoftFactor", softFactor);
            }
            
            // Apply to this object
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = instanceMaterial;
            }
            
            // Apply to children if needed
            if (applyToChildren)
            {
                Renderer[] childRenderers = GetComponentsInChildren<Renderer>(true);
                foreach (Renderer childRenderer in childRenderers)
                {
                    if (childRenderer != renderer) // Skip the parent renderer
                    {
                        childRenderer.material = instanceMaterial;
                    }
                }
            }
        }
        
        /// <summary>
        /// Apply the style with custom colors
        /// </summary>
        public void ApplyStyleWithColors(Color main, Color ambient, Color rim)
        {
            mainColor = main;
            ambientColor = ambient;
            rimColor = rim;
            ApplyStyle();
        }
    }
}
