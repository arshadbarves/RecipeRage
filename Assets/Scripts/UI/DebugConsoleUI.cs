using System;
using System.IO;
using System.Linq;
using Core.Bootstrap;
using Core.Logging;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace UI
{
    /// <summary>
    /// In-game debug console using UI Toolkit
    /// Only available in development builds
    /// </summary>
    public class DebugConsoleUI : MonoBehaviour
    {
        [Header("Input Settings")]
        [SerializeField] private UIDocument _uiDocument;
        [SerializeField] private KeyCode _toggleKey = KeyCode.BackQuote; // Tilde key
        [SerializeField] private int _touchCount = 3; // Number of fingers to trigger
        [SerializeField] private float _touchHoldTime = 1f; // How long to hold touches

        private VisualElement _root;
        private VisualElement _consolePanel;
        private ScrollView _logScrollView;
        private TextField _searchField;
        private DropdownField _levelFilter;
        private DropdownField _categoryFilter;
        private Button _clearButton;
        private Button _exportButton;
        private Button _closeButton;
        private Label _statsLabel;

        private ILoggingService _loggingService;
        private bool _isVisible;
        private LogLevel _currentLevelFilter = LogLevel.Verbose;
        private string _currentCategoryFilter = "All";
        private string _searchQuery = "";
        
        // Mobile touch gesture tracking
        private float _touchStartTime;
        private bool _isTouchGestureActive;

        private void Awake()
        {
#if !DEVELOPMENT_BUILD && !UNITY_EDITOR
            // Destroy in release builds
            Destroy(gameObject);
            return;
#endif
            
            if (_uiDocument == null)
            {
                _uiDocument = GetComponent<UIDocument>();
            }
        }

        private void Start()
        {
#if !DEVELOPMENT_BUILD && !UNITY_EDITOR
            return;
#endif
            
            if (GameBootstrap.Container != null)
            {
                _loggingService = GameBootstrap.Container.Resolve<ILoggingService>();
            }
            
            if (_loggingService == null)
            {
                GameLogger.LogError("LoggingService not found in ServiceContainer!");
                enabled = false;
                return;
            }

            BuildUI();
            _loggingService.OnLogAdded += OnLogAdded;
            
            // Start hidden
            Hide();
            
            // Show mobile instructions
#if UNITY_ANDROID || UNITY_IOS
            GameLogger.Log($"Hold {_touchCount} fingers for {_touchHoldTime}s to toggle console");
#else
            GameLogger.Log($"Press {_toggleKey} to toggle console");
#endif
        }

        private void BuildUI()
        {
            _root = _uiDocument.rootVisualElement;
            _root.Clear();

            // Main console panel
            _consolePanel = new VisualElement();
            _consolePanel.name = "console-panel";
            _consolePanel.style.position = Position.Absolute;
            _consolePanel.style.width = Length.Percent(90);
            _consolePanel.style.height = Length.Percent(80);
            _consolePanel.style.left = Length.Percent(5);
            _consolePanel.style.top = Length.Percent(10);
            _consolePanel.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.95f);
            _consolePanel.style.borderTopLeftRadius = 8;
            _consolePanel.style.borderTopRightRadius = 8;
            _consolePanel.style.borderBottomLeftRadius = 8;
            _consolePanel.style.borderBottomRightRadius = 8;
            _consolePanel.style.paddingTop = 10;
            _consolePanel.style.paddingBottom = 10;
            _consolePanel.style.paddingLeft = 10;
            _consolePanel.style.paddingRight = 10;

            // Header
            var header = CreateHeader();
            _consolePanel.Add(header);

            // Toolbar
            var toolbar = CreateToolbar();
            _consolePanel.Add(toolbar);

            // Stats
            _statsLabel = new Label();
            _statsLabel.style.color = Color.gray;
            _statsLabel.style.fontSize = 12;
            _statsLabel.style.marginBottom = 5;
            _consolePanel.Add(_statsLabel);

            // Log scroll view
            _logScrollView = new ScrollView();
            _logScrollView.style.flexGrow = 1;
            _logScrollView.style.backgroundColor = new Color(0.05f, 0.05f, 0.05f, 1f);
            _logScrollView.style.borderTopWidth = 1;
            _logScrollView.style.borderBottomWidth = 1;
            _logScrollView.style.borderLeftWidth = 1;
            _logScrollView.style.borderRightWidth = 1;
            _logScrollView.style.borderTopColor = Color.gray;
            _logScrollView.style.borderBottomColor = Color.gray;
            _logScrollView.style.borderLeftColor = Color.gray;
            _logScrollView.style.borderRightColor = Color.gray;
            _logScrollView.style.marginTop = 5;
            _consolePanel.Add(_logScrollView);

            _root.Add(_consolePanel);
            
            RefreshLogs();
        }

        private VisualElement CreateHeader()
        {
            var header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.justifyContent = Justify.SpaceBetween;
            header.style.marginBottom = 10;

            var title = new Label("Debug Console");
            title.style.fontSize = 20;
            title.style.color = Color.white;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;

            _closeButton = new Button(() => Hide()) { text = "✕" };
            _closeButton.style.width = 30;
            _closeButton.style.height = 30;
            _closeButton.style.fontSize = 18;

            header.Add(title);
            header.Add(_closeButton);

            return header;
        }

        private VisualElement CreateToolbar()
        {
            var toolbar = new VisualElement();
            toolbar.style.flexDirection = FlexDirection.Row;
            toolbar.style.marginBottom = 10;
            toolbar.style.flexWrap = Wrap.Wrap;

            // Search field
            _searchField = new TextField("Search:");
            _searchField.style.flexGrow = 1;
            _searchField.style.minWidth = 200;
            _searchField.RegisterValueChangedCallback(evt =>
            {
                _searchQuery = evt.newValue;
                RefreshLogs();
            });
            toolbar.Add(_searchField);

            // Level filter
            var levels = Enum.GetNames(typeof(LogLevel)).ToList();
            levels.Insert(0, "All");
            _levelFilter = new DropdownField("Level:", levels, 0);
            _levelFilter.style.minWidth = 150;
            _levelFilter.RegisterValueChangedCallback(evt =>
            {
                _currentLevelFilter = evt.newValue == "All" 
                    ? LogLevel.Verbose 
                    : (LogLevel)Enum.Parse(typeof(LogLevel), evt.newValue);
                RefreshLogs();
            });
            toolbar.Add(_levelFilter);

            // Category filter
            var categories = GetCategories();
            _categoryFilter = new DropdownField("Category:", categories, 0);
            _categoryFilter.style.minWidth = 150;
            _categoryFilter.RegisterValueChangedCallback(evt =>
            {
                _currentCategoryFilter = evt.newValue;
                RefreshLogs();
            });
            toolbar.Add(_categoryFilter);

            // Clear button
            _clearButton = new Button(() =>
            {
                _loggingService.ClearLogs();
                RefreshLogs();
            }) { text = "Clear" };
            _clearButton.style.minWidth = 80;
            toolbar.Add(_clearButton);

            // Export button
            _exportButton = new Button(ExportLogs) { text = "Export" };
            _exportButton.style.minWidth = 80;
            toolbar.Add(_exportButton);

            return toolbar;
        }

        private void RefreshLogs()
        {
            _logScrollView.Clear();

            var logs = _loggingService.GetLogs();
            
            // Apply filters
            var filteredLogs = logs.Where(log =>
            {
                if (_levelFilter.value != "All" && log.Level < _currentLevelFilter)
                    return false;
                    
                if (_currentCategoryFilter != "All" && log.Category != _currentCategoryFilter)
                    return false;
                    
                if (!string.IsNullOrEmpty(_searchQuery) && 
                    !log.Message.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase))
                    return false;

                return true;
            }).ToArray();

            // Update stats
            _statsLabel.text = $"Showing {filteredLogs.Length} of {logs.Length} logs";

            // Add log entries
            foreach (var log in filteredLogs)
            {
                var logElement = CreateLogElement(log);
                _logScrollView.Add(logElement);
            }

            // Scroll to bottom
            _logScrollView.ScrollTo(_logScrollView.contentContainer.Children().LastOrDefault());
        }

        private VisualElement CreateLogElement(LogEntry log)
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.paddingTop = 4;
            container.style.paddingBottom = 4;
            container.style.paddingLeft = 8;
            container.style.paddingRight = 8;
            container.style.borderBottomWidth = 1;
            container.style.borderBottomColor = new Color(0.2f, 0.2f, 0.2f, 1f);

            // Timestamp
            var timestamp = new Label(log.Timestamp);
            timestamp.style.color = Color.gray;
            timestamp.style.fontSize = 10;
            timestamp.style.minWidth = 150;
            container.Add(timestamp);

            // Level badge
            var levelBadge = new Label($"[{log.Level}]");
            levelBadge.style.color = GetLevelColor(log.Level);
            levelBadge.style.fontSize = 11;
            levelBadge.style.minWidth = 80;
            levelBadge.style.unityFontStyleAndWeight = FontStyle.Bold;
            container.Add(levelBadge);

            // Category
            var category = new Label($"[{log.Category}]");
            category.style.color = new Color(0.7f, 0.7f, 1f, 1f);
            category.style.fontSize = 11;
            category.style.minWidth = 100;
            container.Add(category);

            // Message
            var message = new Label(log.Message);
            message.style.color = Color.white;
            message.style.fontSize = 12;
            message.style.flexGrow = 1;
            message.style.whiteSpace = WhiteSpace.Normal;
            container.Add(message);

            // Add click to show stack trace
            if (!string.IsNullOrEmpty(log.StackTrace))
            {
                container.RegisterCallback<ClickEvent>(evt =>
                {
                    var stackTrace = new Label(log.StackTrace);
                    stackTrace.style.color = Color.yellow;
                    stackTrace.style.fontSize = 10;
                    stackTrace.style.marginLeft = 20;
                    stackTrace.style.whiteSpace = WhiteSpace.Normal;
                    
                    if (container.childCount == 4)
                    {
                        container.Add(stackTrace);
                    }
                    else
                    {
                        container.RemoveAt(4);
                    }
                });
            }

            return container;
        }

        private Color GetLevelColor(LogLevel level)
        {
            return level switch
            {
                LogLevel.Verbose => Color.gray,
                LogLevel.Info => Color.white,
                LogLevel.Warning => Color.yellow,
                LogLevel.Error => new Color(1f, 0.5f, 0f),
                LogLevel.Critical => Color.red,
                _ => Color.white
            };
        }

        private System.Collections.Generic.List<string> GetCategories()
        {
            var categories = _loggingService.GetLogs()
                .Select(log => log.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToList();
            
            categories.Insert(0, "All");
            return categories;
        }

        private void ExportLogs()
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var fileName = $"GameLogs_{timestamp}.txt";
            var filePath = Path.Combine(Application.persistentDataPath, fileName);
            
            _loggingService.SaveLogsToFile(filePath);
            
            GameLogger.Log($"Logs exported to: {filePath}");
            
            // Show notification (you could add a toast notification here)
            _statsLabel.text = $"Logs exported to: {filePath}";
        }

        private void OnLogAdded(LogEntry log)
        {
            // Only refresh if visible and matches current filters
            if (_isVisible)
            {
                RefreshLogs();
            }
        }

        private void Update()
        {
#if !DEVELOPMENT_BUILD && !UNITY_EDITOR
            return;
#endif
            
            // Keyboard toggle (for editor/PC)
            if (Input.GetKeyDown(_toggleKey))
            {
                Toggle();
            }
            
            // Mobile touch gesture (3+ finger hold)
            HandleMobileTouchGesture();
        }
        
        private void HandleMobileTouchGesture()
        {
            int currentTouchCount = Input.touchCount;
            
            // Check if we have the required number of touches
            if (currentTouchCount >= _touchCount)
            {
                // Start tracking if not already
                if (!_isTouchGestureActive)
                {
                    _isTouchGestureActive = true;
                    _touchStartTime = Time.time;
                }
                
                // Check if held long enough
                float holdDuration = Time.time - _touchStartTime;
                if (holdDuration >= _touchHoldTime)
                {
                    Toggle();
                    _isTouchGestureActive = false; // Reset to prevent repeated toggles
                }
            }
            else
            {
                // Reset if touches released
                _isTouchGestureActive = false;
            }
        }

        public void Show()
        {
            _isVisible = true;
            _consolePanel.style.display = DisplayStyle.Flex;
            RefreshLogs();
        }

        public void Hide()
        {
            _isVisible = false;
            _consolePanel.style.display = DisplayStyle.None;
        }

        public void Toggle()
        {
            if (_isVisible)
                Hide();
            else
                Show();
        }

        private void OnDestroy()
        {
            if (_loggingService != null)
            {
                _loggingService.OnLogAdded -= OnLogAdded;
            }
        }
    }
}
