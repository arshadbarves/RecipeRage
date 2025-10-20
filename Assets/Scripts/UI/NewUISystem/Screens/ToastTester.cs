using Core.Bootstrap;
using Cysharp.Threading.Tasks;
using UI.UISystem.Screens;
using UnityEngine;

namespace UI.UISystem.Testing
{
    /// <summary>
    /// Simple test script to verify toast notifications are working
    /// Add this to a GameObject or call from Unity console
    /// </summary>
    public class ToastTester : MonoBehaviour
    {
        [ContextMenu("Test Success Toast")]
        public void TestSuccessToast()
        {
            ShowToast("Login successful!", ToastType.Success).Forget();
        }

        [ContextMenu("Test Error Toast")]
        public void TestErrorToast()
        {
            ShowToast("Connection failed", ToastType.Error).Forget();
        }

        [ContextMenu("Test Info Toast")]
        public void TestInfoToast()
        {
            ShowToast("Loading data...", ToastType.Info).Forget();
        }

        [ContextMenu("Test Warning Toast")]
        public void TestWarningToast()
        {
            ShowToast("Low battery warning", ToastType.Warning).Forget();
        }

        [ContextMenu("Test All Toasts")]
        public void TestAllToasts()
        {
            TestAllToastsAsync().Forget();
        }

        private async UniTask TestAllToastsAsync()
        {
            var uiService = GameBootstrap.Services?.UIService;
            if (uiService == null)
            {
                Debug.LogError("[ToastTester] UIService not available");
                return;
            }

            Debug.Log("[ToastTester] Testing all toast types...");

            await uiService.ShowToast("Success toast test", ToastType.Success, 2f);
            await UniTask.Delay(500);
            
            await uiService.ShowToast("Error toast test", ToastType.Error, 2f);
            await UniTask.Delay(500);
            
            await uiService.ShowToast("Info toast test", ToastType.Info, 2f);
            await UniTask.Delay(500);
            
            await uiService.ShowToast("Warning toast test", ToastType.Warning, 2f);

            Debug.Log("[ToastTester] All toasts tested!");
        }

        private async UniTask ShowToast(string message, ToastType type)
        {
            var uiService = GameBootstrap.Services?.UIService;
            if (uiService == null)
            {
                Debug.LogError("[ToastTester] UIService not available");
                return;
            }

            Debug.Log($"[ToastTester] Showing {type} toast: {message}");
            await uiService.ShowToast(message, type, 3f);
        }

        // Keyboard shortcuts for quick testing
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                TestSuccessToast();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                TestErrorToast();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                TestInfoToast();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                TestWarningToast();
            }
            else if (Input.GetKeyDown(KeyCode.T))
            {
                TestAllToasts();
            }
        }
    }
}
