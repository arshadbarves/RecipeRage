using UnityEngine;
using UnityEngine.UIElements;
using System.IO;

public class LoginManager : MonoBehaviour
{
    public static LoginManager Instance;
    private VisualElement loginUI;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Load login UI from UXML
        var uiDocument = GetComponent<UIDocument>();
        loginUI = uiDocument.rootVisualElement.Q("LoginUI");

        // Check for saved credentials
        CheckForSavedCredentials();
    }

    public void ShowLoginUI()
    {
        loginUI.style.display = DisplayStyle.Flex;
    }

    private void CheckForSavedCredentials()
    {
        string path = Path.Combine(Application.persistentDataPath, "credentials.txt");
        if (File.Exists(path))
        {
            string[] credentials = File.ReadAllLines(path);
            if (credentials.Length == 2)
            {
                // Auto-login with saved credentials
                AutoLogin(credentials[0], credentials[1]);
            }
        }
        else
        {
            // Show login UI if no credentials found
            ShowLoginUI();
        }
    }

    private void AutoLogin(string username, string password)
    {
        // Logic to authenticate user
        Debug.Log($"Auto-logging in {username}");
        // Proceed to main application UI
    }

    public void SaveCredentials(string username, string password)
    {
        string path = Path.Combine(Application.persistentDataPath, "credentials.txt");
        File.WriteAllLines(path, new[] { username, password });
    }
} 