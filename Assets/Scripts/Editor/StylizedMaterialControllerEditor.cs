using UnityEngine;
using UnityEditor;
using RecipeRage.Utilities;

namespace RecipeRage.Editor
{
    /// <summary>
    /// Custom editor for the StylizedMaterialController component.
    /// </summary>
    [CustomEditor(typeof(StylizedMaterialController))]
    public class StylizedMaterialControllerEditor : UnityEditor.Editor
    {
        // Serialized properties
        private SerializedProperty _stylizedMaterial;
        private SerializedProperty _createMaterialInstance;
        private SerializedProperty _applyToChildren;
        private SerializedProperty _color;
        private SerializedProperty _ambientColor;
        private SerializedProperty _ambientIntensity;
        private SerializedProperty _lightDirection;
        private SerializedProperty _lightColor;
        private SerializedProperty _lightIntensity;
        private SerializedProperty _shadowSoftness;
        private SerializedProperty _specularColor;
        private SerializedProperty _glossiness;
        private SerializedProperty _specularIntensity;
        private SerializedProperty _metallic;
        private SerializedProperty _roughness;
        private SerializedProperty _rimColor;
        private SerializedProperty _rimPower;
        private SerializedProperty _rimIntensity;
        private SerializedProperty _updateInPlayMode;
        private SerializedProperty _updateInEditMode;
        
        // Foldout states
        private bool _showBaseProperties = true;
        private bool _showAmbientProperties = true;
        private bool _showLightProperties = true;
        private bool _showSpecularProperties = true;
        private bool _showMaterialProperties = true;
        private bool _showRimProperties = true;
        private bool _showUpdateSettings = false;
        
        private void OnEnable()
        {
            // Find the serialized properties
            _stylizedMaterial = serializedObject.FindProperty("_stylizedMaterial");
            _createMaterialInstance = serializedObject.FindProperty("_createMaterialInstance");
            _applyToChildren = serializedObject.FindProperty("_applyToChildren");
            _color = serializedObject.FindProperty("_color");
            _ambientColor = serializedObject.FindProperty("_ambientColor");
            _ambientIntensity = serializedObject.FindProperty("_ambientIntensity");
            _lightDirection = serializedObject.FindProperty("_lightDirection");
            _lightColor = serializedObject.FindProperty("_lightColor");
            _lightIntensity = serializedObject.FindProperty("_lightIntensity");
            _shadowSoftness = serializedObject.FindProperty("_shadowSoftness");
            _specularColor = serializedObject.FindProperty("_specularColor");
            _glossiness = serializedObject.FindProperty("_glossiness");
            _specularIntensity = serializedObject.FindProperty("_specularIntensity");
            _metallic = serializedObject.FindProperty("_metallic");
            _roughness = serializedObject.FindProperty("_roughness");
            _rimColor = serializedObject.FindProperty("_rimColor");
            _rimPower = serializedObject.FindProperty("_rimPower");
            _rimIntensity = serializedObject.FindProperty("_rimIntensity");
            _updateInPlayMode = serializedObject.FindProperty("_updateInPlayMode");
            _updateInEditMode = serializedObject.FindProperty("_updateInEditMode");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            StylizedMaterialController controller = (StylizedMaterialController)target;
            
            // Material references
            EditorGUILayout.LabelField("Material References", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_stylizedMaterial);
            EditorGUILayout.PropertyField(_createMaterialInstance);
            EditorGUILayout.PropertyField(_applyToChildren);
            
            if (_stylizedMaterial.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("Please assign a stylized material!", MessageType.Warning);
            }
            
            EditorGUILayout.Space();
            
            // Base properties
            _showBaseProperties = EditorGUILayout.Foldout(_showBaseProperties, "Base Properties", true, EditorStyles.foldoutHeader);
            if (_showBaseProperties)
            {
                EditorGUILayout.PropertyField(_color);
            }
            
            EditorGUILayout.Space();
            
            // Ambient lighting
            _showAmbientProperties = EditorGUILayout.Foldout(_showAmbientProperties, "Ambient Lighting", true, EditorStyles.foldoutHeader);
            if (_showAmbientProperties)
            {
                EditorGUILayout.PropertyField(_ambientColor);
                EditorGUILayout.PropertyField(_ambientIntensity);
            }
            
            EditorGUILayout.Space();
            
            // Fake directional light
            _showLightProperties = EditorGUILayout.Foldout(_showLightProperties, "Fake Directional Light", true, EditorStyles.foldoutHeader);
            if (_showLightProperties)
            {
                EditorGUILayout.PropertyField(_lightDirection);
                EditorGUILayout.PropertyField(_lightColor);
                EditorGUILayout.PropertyField(_lightIntensity);
                
                // Check if the shader supports shadow softness
                Material material = _stylizedMaterial.objectReferenceValue as Material;
                if (material != null && material.HasProperty("_ShadowSoftness"))
                {
                    EditorGUILayout.PropertyField(_shadowSoftness);
                }
            }
            
            EditorGUILayout.Space();
            
            // Specular
            _showSpecularProperties = EditorGUILayout.Foldout(_showSpecularProperties, "Specular", true, EditorStyles.foldoutHeader);
            if (_showSpecularProperties)
            {
                EditorGUILayout.PropertyField(_specularColor);
                EditorGUILayout.PropertyField(_glossiness);
                EditorGUILayout.PropertyField(_specularIntensity);
            }
            
            EditorGUILayout.Space();
            
            // Material properties
            Material material = _stylizedMaterial.objectReferenceValue as Material;
            if (material != null && material.HasProperty("_Metallic"))
            {
                _showMaterialProperties = EditorGUILayout.Foldout(_showMaterialProperties, "Material Properties", true, EditorStyles.foldoutHeader);
                if (_showMaterialProperties)
                {
                    EditorGUILayout.PropertyField(_metallic);
                    EditorGUILayout.PropertyField(_roughness);
                }
                
                EditorGUILayout.Space();
            }
            
            // Rim effect
            _showRimProperties = EditorGUILayout.Foldout(_showRimProperties, "Rim Effect", true, EditorStyles.foldoutHeader);
            if (_showRimProperties)
            {
                EditorGUILayout.PropertyField(_rimColor);
                EditorGUILayout.PropertyField(_rimPower);
                
                // Check if the shader supports rim intensity
                if (material != null && material.HasProperty("_RimIntensity"))
                {
                    EditorGUILayout.PropertyField(_rimIntensity);
                }
            }
            
            EditorGUILayout.Space();
            
            // Update settings
            _showUpdateSettings = EditorGUILayout.Foldout(_showUpdateSettings, "Update Settings", true, EditorStyles.foldoutHeader);
            if (_showUpdateSettings)
            {
                EditorGUILayout.PropertyField(_updateInPlayMode);
                EditorGUILayout.PropertyField(_updateInEditMode);
            }
            
            EditorGUILayout.Space();
            
            // Buttons
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Update Material"))
            {
                controller.UpdateMaterialProperties();
                EditorUtility.SetDirty(target);
            }
            
            if (GUILayout.Button("Create Material Instance"))
            {
                controller.CreateMaterialInstance();
                EditorUtility.SetDirty(target);
            }
            
            EditorGUILayout.EndHorizontal();
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}
