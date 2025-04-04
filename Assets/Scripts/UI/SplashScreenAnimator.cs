using System.Collections;
            using UnityEngine;
            using UnityEngine.UIElements;
            using UnityEngine.SceneManagement;
            
            public class SplashScreenAnimator : MonoBehaviour
            {
                [SerializeField] private UIDocument _uiDocument;
                [SerializeField] private string _nextSceneName = "MainMenu";
                [SerializeField] private float _totalDuration = 5.0f;
            
                // UI Elements
                private VisualElement _root;
                private VisualElement _background;
                private VisualElement _logoContainer;
                private Label _studioNameLabel;
            
                // Animation parameters
                private const float BG_FADE_IN_DURATION = 1.0f;
                private const float LOGO_SCALE_DURATION = 1.2f;
                private const float STUDIO_NAME_FADE_DURATION = 1.0f;
                private const float HOLD_DURATION = 1.5f;
                private const float FADE_OUT_DURATION = 0.8f;
            
                private void Start()
                {
                    SetupUI();
                    StartCoroutine(PlaySplashAnimation());
                }
            
                private void SetupUI()
                {
                    if (_uiDocument == null)
                    {
                        _uiDocument = GetComponent<UIDocument>();
                        if (_uiDocument == null)
                        {
                            Debug.LogError("SplashScreenAnimator: UIDocument not found!");
                            return;
                        }
                    }
            
                    _root = _uiDocument.rootVisualElement;
                    _background = _root.Q<VisualElement>("Background");
                    _logoContainer = _root.Q<VisualElement>("LogoContainer");
                    _studioNameLabel = _root.Q<Label>("StudioName");
            
                    // Set initial state
                    if (_background != null)
                        _background.style.opacity = 0;
            
                    if (_logoContainer != null)
                    {
                        _logoContainer.style.opacity = 0;
                        _logoContainer.style.scale = new Scale(new Vector2(0.5f, 0.5f));
                    }
            
                    if (_studioNameLabel != null)
                        _studioNameLabel.style.opacity = 0;
                }
            
                private IEnumerator PlaySplashAnimation()
                {
                    // 1. Fade in background
                    if (_background != null)
                    {
                        float startTime = Time.time;
                        while (Time.time < startTime + BG_FADE_IN_DURATION)
                        {
                            float t = (Time.time - startTime) / BG_FADE_IN_DURATION;
                            _background.style.opacity = Mathf.SmoothStep(0, 1, t);
                            yield return null;
                        }
                        _background.style.opacity = 1;
                    }
            
                    yield return new WaitForSeconds(0.3f);
            
                    // 2. Animate logo scale and fade in
                    if (_logoContainer != null)
                    {
                        float startTime = Time.time;
                        while (Time.time < startTime + LOGO_SCALE_DURATION)
                        {
                            float t = (Time.time - startTime) / LOGO_SCALE_DURATION;
                            float easedT = EaseOutCubic(t);
                            _logoContainer.style.scale = new Scale(new Vector2(
                                Mathf.Lerp(0.5f, 1.0f, easedT),
                                Mathf.Lerp(0.5f, 1.0f, easedT)
                            ));
                            _logoContainer.style.opacity = Mathf.Lerp(0, 1, easedT);
                            yield return null;
                        }
                        _logoContainer.style.scale = new Scale(new Vector2(1, 1));
                        _logoContainer.style.opacity = 1;
                    }
            
                    yield return new WaitForSeconds(0.2f);
            
                    // 3. Fade in studio name
                    if (_studioNameLabel != null)
                    {
                        float startTime = Time.time;
                        while (Time.time < startTime + STUDIO_NAME_FADE_DURATION)
                        {
                            float t = (Time.time - startTime) / STUDIO_NAME_FADE_DURATION;
                            _studioNameLabel.style.opacity = Mathf.SmoothStep(0, 1, t);
                            yield return null;
                        }
                        _studioNameLabel.style.opacity = 1;
                    }
            
                    // 4. Hold animation
                    yield return new WaitForSeconds(HOLD_DURATION);
            
                    // 5. Fade out everything
                    float fadeOutStartTime = Time.time;
                    while (Time.time < fadeOutStartTime + FADE_OUT_DURATION)
                    {
                        float t = (Time.time - fadeOutStartTime) / FADE_OUT_DURATION;
                        float easedT = EaseInOutSine(t);
            
                        if (_background != null)
                            _background.style.opacity = Mathf.Lerp(1, 0, easedT);
            
                        if (_logoContainer != null)
                            _logoContainer.style.opacity = Mathf.Lerp(1, 0, easedT);
            
                        if (_studioNameLabel != null)
                            _studioNameLabel.style.opacity = Mathf.Lerp(1, 0, easedT);
            
                        yield return null;
                    }
            
                    // 6. Load next scene
                    SceneManager.LoadScene(_nextSceneName);
                }
            
                // Easing Functions
                private float EaseOutCubic(float t)
                {
                    return 1 - Mathf.Pow(1 - t, 3);
                }
            
                private float EaseInOutSine(float t)
                {
                    return -(Mathf.Cos(Mathf.PI * t) - 1) / 2;
                }
            }