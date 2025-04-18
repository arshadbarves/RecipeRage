/*
* Copyright (c) 2021 PlayEveryWare
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.EventSystems;
    using Epic.OnlineServices.Logging;

    public class UIDebugLog : MonoBehaviour
    {
        [Header("Debug Log UI")]
        public bool DisableOnScreenLog = false;
        public const int MAX_LINES_TO_DISPLAY = 7;
        public RectTransform DebugLogContainer;
        public Text UIDebugLogText;
        public ScrollRect ScrollRect;

        private Queue<string> logCacheList = new Queue<string>();

        /// <summary>
        /// The <see cref="UIDebugLogText"/> cannot generate a mesh that has more than "65000" vertices.
        /// Particularly long logged messages can add up to past this value.
        /// This would result in the log throwing an ArgumentException every time it is updated.
        /// This value is an approximation of how long of a string a text element can hold.
        /// </summary>
        private const int MaximumLogStringLength = 10000;

        /// <summary>
        /// Indicates the total number of logs that can be in the cache.
        /// </summary>
        private const int MaximumLogsInCache = 100;

        /// <summary>
        /// Stores the current total string length of all the contents of the
        /// log cache.
        /// </summary>
        private int _currentLogStringLength;

        private bool _dirty = false;
        private string logCache = string.Empty;
        private string textFilter = string.Empty;
        //private bool userDrag = false;
        private bool userScroll = false;

        private float deltaTime_FPS;
        public Text FPSValue;

        public UISampleSceneUIContainer DemoSceneContainer;

        public GameObject[] OptionElements;
        public Button LogLevelMenuButton;
        private bool optionsVisible;

        private float initialFlexHeight;
        private bool visible;
        private bool expanded;

        [Header("Log Level Menu")]
        public ScrollRect LogLevelScrollView;
        public Transform LogLevelContentContainer;
        public UIDebugLogLevelMenuItem LogLevelTemplate;
        private UIDebugLogLevelMenuItem allCategoriesMenuItem;
        private List<UIDebugLogLevelMenuItem> logLevelMenuItems;
        private bool ignoreLogLevelChange;

        private void Start()
        {
            expanded = false;
            visible = true;

            ignoreLogLevelChange = true;
            logLevelMenuItems = new List<UIDebugLogLevelMenuItem>();
            BuildLogLevelMenu();
            ignoreLogLevelChange = false;

            LogLevelScrollView?.gameObject.SetActive(false);
        }

        public void OnScollDragBegin()
        {
            //userDrag = true;
            userScroll = true;
        }

        public void OnScollDragEnd()
        {
            //userDrag = false;
            userScroll = ScrollRect.velocity.y != 0 && ScrollRect.verticalNormalizedPosition > 0;
        }

        public void OnScrollValueChanged(Vector2 value)
        {
            if (userScroll && value.y <= 0)
            {
                userScroll = false;
            }
        }

        private void BuildLogLevelMenu()
        {
            if (LogLevelTemplate == null)
            {
                return;
            }

            LogLevelTemplate.InitDropdown();

            allCategoriesMenuItem = CreateLogCategoryItem(LogCategory.AllCategories);
            logLevelMenuItems.Add(allCategoriesMenuItem);
            foreach (LogCategory cat in Enum.GetValues(typeof(LogCategory)))
            {
                if (cat != LogCategory.AllCategories)
                {
                    logLevelMenuItems.Add(CreateLogCategoryItem(cat));
                }
            }

            for (int i = 0; i < logLevelMenuItems.Count; ++i)
            {
                logLevelMenuItems[i].LogLevelDropdown.navigation = new Navigation()
                {
                    mode = Navigation.Mode.Explicit,
                    selectOnRight = LogLevelMenuButton,
                    selectOnUp = logLevelMenuItems[i == 0 ? logLevelMenuItems.Count - 1 : i - 1].LogLevelDropdown,
                    selectOnDown = logLevelMenuItems[i == logLevelMenuItems.Count - 1 ? 0 : i + 1].LogLevelDropdown,
                };
            }
        }

        private UIDebugLogLevelMenuItem CreateLogCategoryItem(LogCategory Category)
        {
            var newItem = Instantiate(LogLevelTemplate, LogLevelContentContainer);
            newItem.SetCategory(Category);
            newItem.name = $"{Category}LogLevel";
            LogLevel catLevel = EOSManager.Instance.GetLogLevel(Category);
            newItem.SetLevel(catLevel);
            newItem.gameObject.SetActive(true);
            return newItem;
        }

        public void OnLogLevelChanged(LogCategory Category, LogLevel Level)
        {
            if (ignoreLogLevelChange)
            {
                //ignore value change events while menu is being initialized or changed here
                return;
            }

            EOSManager.Instance.SetLogLevel(Category, Level);
            ignoreLogLevelChange = true;
            if (Category == LogCategory.AllCategories)
            {
                //update all levels if AllCategories was changed
                foreach (var item in logLevelMenuItems)
                {
                    var cat = item.GetCategory();
                    if (cat != LogCategory.AllCategories)
                    {
                        item.SetLevel(EOSManager.Instance.GetLogLevel(cat));
                    }
                }
            }
            else
            {
                allCategoriesMenuItem.SetLevel(EOSManager.Instance.GetLogLevel(LogCategory.AllCategories));
            }
            ignoreLogLevelChange = false;
        }

        public void ScrollToLogLevelItem(UIDebugLogLevelMenuItem item)
        {
            float itemIndex = logLevelMenuItems.IndexOf(item);
            if (itemIndex > -1)
            {
                float position = 1.0f - ((float)itemIndex / (logLevelMenuItems.Count - 1));
                LogLevelScrollView.verticalNormalizedPosition = position;
            }
        }

        public void ToggleLogLevelMenu()
        {
            if (!LogLevelScrollView.gameObject.activeSelf && logLevelMenuItems.Count == 0)
            {
                return;
            }

            LogLevelScrollView.gameObject.SetActive(!LogLevelScrollView.gameObject.activeSelf);

            if (LogLevelScrollView.gameObject.activeSelf)
            {
                foreach (var item in logLevelMenuItems)
                {
                    var cat = item.GetCategory();
                    item.SetLevel(EOSManager.Instance.GetLogLevel(cat));
                }

                EventSystem.current.SetSelectedGameObject(logLevelMenuItems[0].LogLevelDropdown.gameObject);
            }
        }

        void OnEnable()
        {
            if(!DisableOnScreenLog)
            {
                Application.logMessageReceived += UpdateLogCache;
            }
            else
            {
                UIDebugLogText.text = "<I>OnScreen Logging Disabled</I>";
            }

            foreach (var element in OptionElements)
            {
                element.SetActive(false);
            }
            optionsVisible = false;
        }

        void OnDisable()
        {
            if (!DisableOnScreenLog)
            {
                Application.logMessageReceived -= UpdateLogCache;
            }
            else
            {
                UIDebugLogText.text = string.Empty;
            }
        }

        void UpdateLogCache(string entry, string stackTrace, LogType type)
        {
            string logEntry = System.DateTime.UtcNow.ToString("HH:mm:ss.fff") + " [" + type + "] : " + entry;
            string color = string.Empty;

            switch (type)
            {
                case LogType.Warning:
                    color = "#FFFF00"; // Yellow
                    break;
                case LogType.Error:
                case LogType.Exception:
                    color = "#FF0000"; // Red
                    break;
            }

            if (!string.IsNullOrEmpty(color))
            {
                logEntry = "<color=" + color + ">" + logEntry + "</color>";
            }

            // Add the length of the new log entry to the length of the total 
            // log cache contents.
            _currentLogStringLength += logEntry.Length;

            // While there are items in the log cache list to remove, and while
            // the current log string length is greater than the maximum string
            // length allowed, remove items until the string length is within
            // limits, or there are no more logs to remove from the cache.
            while (logCacheList.Count != 0 && _currentLogStringLength > MaximumLogStringLength)
            {
                _currentLogStringLength -= logCacheList.Dequeue().Length;
            }

            // Add the new log entry to the cache list
            logCacheList.Enqueue(logEntry);

            // If the log cache list has more than the maximum items, than
            // remove items from the cache and update the current log string
            // length.
            if (logCacheList.Count > MaximumLogsInCache)
            {
                _currentLogStringLength -= logCacheList.Dequeue().Length;
            }

            _dirty = true;
        }

        string GetLastEntries()
        {
            if(_dirty)
            {
                logCache = string.Empty;

                foreach (string logEntry in logCacheList)
                {
                    if (textFilter != string.Empty && !logEntry.ToLower().Contains(textFilter))
                    {
                        continue;
                    }

                    logCache += '\n' + logEntry;
                }
                _dirty = false;
            }

            return logCache;
        }

        public void OnTextFilterEdit(string newFilter)
        {
            textFilter = newFilter.Trim().ToLower();
            _dirty = true;
        }

        public void ToggleOptions()
        {
            optionsVisible = !optionsVisible;
            foreach (var element in OptionElements)
            {
                element.SetActive(optionsVisible);
            }
        }

        public void ToggleExpand()
        {
            bool shouldScrollToBottom = !userScroll && ScrollRect.verticalNormalizedPosition <= 0.000001f;
            expanded = !expanded;

            if (!visible)
            {
                ToggleLogVisibility();
            }

            DemoSceneContainer.SetVisible(!expanded);

            float optionsPivotY = expanded ? 1 : 0;
            //OptionsBar.pivot = new Vector2(OptionsBar.pivot.x, optionsPivotY);

            if (shouldScrollToBottom)
            {
                Invoke("ScrollToBottom", 0.1f);
            }
        }

        public void ToggleLogVisibility()
        {
            visible = !visible;
            DebugLogContainer.gameObject.SetActive(visible);
            if (!visible)
            {
                DemoSceneContainer.SetVisible(true);
            }
            else if(expanded)
            {
                DemoSceneContainer.SetVisible(false);
            }
        }

        private void Update()
        {
            if (!DisableOnScreenLog && _dirty)
            {
                UIDebugLogText.text = GetLastEntries();
                if (!userScroll)
                {
                    Invoke(nameof(ScrollToBottom), 0.1f);
                }
            }

            // FPS
            if (FPSValue != null)
            {
                deltaTime_FPS += (Time.deltaTime - deltaTime_FPS) * 0.1f;

                if (deltaTime_FPS != 0)
                {
                    float fps = 1.0f / deltaTime_FPS;
                    FPSValue.text = Mathf.Ceil(fps).ToString();
                }
                else
                {
                    FPSValue.text = nameof(float.NaN);
                }
            }
        }

        private void ScrollToBottom()
        {
            ScrollRect.verticalNormalizedPosition = 0;
        }
    }
}