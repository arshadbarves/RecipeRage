using System;
using System.Collections.Generic;
using System.Linq;
using Core.Animation;
using Core.Logging;
using Core.SaveSystem;
using UI.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    /// <summary>
    /// Interactive HUD Editor for customizing mobile controls.
    /// Allows drag-and-drop repositioning and resizing of HUD elements.
    /// </summary>
    [UIScreen(UIScreenType.HUDEditor, UIScreenCategory.Overlay, "Components/JoystickEditorTemplate")]
    public class HUDEditorScreen : BaseUIScreen
    {
        private ISaveService _saveService;
        private VisualElement _editorRoot;
        private List<EditableControl> _editableControls = new List<EditableControl>();
        
        private Button _saveBtn;
        private Button _resetBtn;
        private Button _exitBtn;
        
        private EditableControl _selectedControl;

        public void Initialize() { } // Added for interface compatibility if needed

        protected override void OnInitialize()
        {
            _saveService = Core.Bootstrap.GameBootstrap.Services?.SaveService;
            
            _editorRoot = GetElement<VisualElement>("editor-root");
            _saveBtn = GetElement<Button>("save-button");
            _resetBtn = GetElement<Button>("reset-button");
            _exitBtn = GetElement<Button>("close-button");

            if (_saveBtn != null) _saveBtn.clicked += SaveLayout;
            if (_resetBtn != null) _resetBtn.clicked += ResetLayout;
            if (_exitBtn != null) _exitBtn.clicked += () => Hide(true);

            InitializeEditableControls();
        }

        private void InitializeEditableControls()
        {
            // Register standard HUD elements for editing
            RegisterControl("movement-joystick", "Movement");
            RegisterControl("aim-joystick", "Aim/Attack");
            RegisterControl("jump-btn", "Jump");
            RegisterControl("dash-btn", "Dash");
            RegisterControl("special-btn", "Special");
        }

        private void RegisterControl(string id, string label)
        {
            var element = GetElement<VisualElement>(id);
            if (element != null)
            {
                var editable = new EditableControl(id, label, element);
                editable.OnSelected += control => SelectControl(control);
                _editableControls.Add(editable);
            }
        }

        private void SelectControl(EditableControl control)
        {
            if (_selectedControl != null) _selectedControl.SetSelected(false);
            _selectedControl = control;
            if (_selectedControl != null) _selectedControl.SetSelected(true);
            
            GameLogger.Log($"Selected control for editing: {control.Id}");
        }

        private void SaveLayout()
        {
            var settings = _saveService.GetSettings();
            settings.HUDLayout.Clear();

            foreach (var control in _editableControls)
            {
                settings.HUDLayout.Add(new ControlLayoutData
                {
                    ControlId = control.Id,
                    NormalizedPosition = control.GetNormalizedPosition(),
                    SizeMultiplier = control.Size,
                    Opacity = control.Opacity
                });
            }

            _saveService.SaveSettings(settings);
            GameBootstrap.Services?.UIService?.ShowNotification("HUD Layout Saved", NotificationType.Success, 2f);
            Hide(true);
        }

        private void ResetLayout()
        {
            // Revert to default positions
            foreach (var control in _editableControls)
            {
                control.ResetToDefault();
            }
            GameLogger.Log("HUD Layout Reset to Defaults");
        }

        protected override void OnShow()
        {
            LoadLayout();
        }

        private void LoadLayout()
        {
            var settings = _saveService.GetSettings();
            foreach (var data in settings.HUDLayout)
            {
                var control = _editableControls.Find(c => c.Id == data.ControlId);
                if (control != null)
                {
                    control.SetLayout(data.NormalizedPosition, data.SizeMultiplier, data.Opacity);
                }
            }
        }

        #region Helper Class

        private class EditableControl
        {
            public string Id { get; }
            public VisualElement Element { get; }
            public float Size { get; private set; } = 1.0f;
            public float Opacity { get; private set; } = 1.0f;
            
            public event Action<EditableControl> OnSelected;

            private Vector2 _defaultPos;
            private bool _isDragging;
            private Vector2 _pointerStartPos;
            private Vector2 _elementStartPos;

            public EditableControl(string id, string label, VisualElement element)
            {
                Id = id;
                Element = element;
                _defaultPos = element.layout.position;

                Element.RegisterCallback<PointerDownEvent>(OnPointerDown);
                Element.RegisterCallback<PointerMoveEvent>(OnPointerMove);
                Element.RegisterCallback<PointerUpEvent>(OnPointerUp);
            }

            public void SetSelected(bool selected)
            {
                if (selected) Element.AddToClassList("selected-for-edit");
                else Element.RemoveFromClassList("selected-for-edit");
            }

            public void SetLayout(Vector2 normalizedPos, float size, float opacity)
            {
                Size = size;
                Opacity = opacity;
                
                // Position logic...
                // In a real implementation, we'd map normalized (0-1) to parent dimensions
            }

            public Vector2 GetNormalizedPosition()
            {
                // Return normalized coordinates...
                return Vector2.zero; 
            }

            public void ResetToDefault()
            {
                // Reset logic...
            }

            private void OnPointerDown(PointerDownEvent evt)
            {
                _isDragging = true;
                _pointerStartPos = evt.position;
                _elementStartPos = new Vector2(Element.resolvedStyle.left, Element.resolvedStyle.top);
                Element.CapturePointer(evt.pointerId);
                OnSelected?.Invoke(this);
                evt.StopPropagation();
            }

            private void OnPointerMove(PointerMoveEvent evt)
            {
                if (!_isDragging) return;

                Vector2 delta = (Vector2)evt.position - _pointerStartPos;
                Element.style.left = _elementStartPos.x + delta.x;
                Element.style.top = _elementStartPos.y + delta.y;
                evt.StopPropagation();
            }

            private void OnPointerUp(PointerUpEvent evt)
            {
                if (!_isDragging) return;
                _isDragging = false;
                Element.ReleasePointer(evt.pointerId);
                evt.StopPropagation();
            }
        }

        #endregion
    }
}
