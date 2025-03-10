using UnityEngine;
using UnityEngine.UIElements;

public class SplashScreen : MonoBehaviour
{
    private VisualElement rootElement;

    void Start()
    {
        // Load the UI from UXML
        var uiDocument = GetComponent<UIDocument>();
        rootElement = uiDocument.rootVisualElement;

        // Show splash screen
        ShowSplashScreen();
    }

    private void ShowSplashScreen()
    {
        // Logic to display splash screen
        rootElement.style.display = DisplayStyle.Flex;

        // Simulate loading time
        Invoke(nameof(LoadLoginScreen), 3f); // Show for 3 seconds
    }

    private void LoadLoginScreen()
    {
        // Hide splash screen and load login UI
        rootElement.style.display = DisplayStyle.None;
        LoginManager.Instance.ShowLoginUI();
    }
} 