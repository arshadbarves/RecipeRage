using Epic.OnlineServices;
using Epic.OnlineServices.Connect;
using PlayEveryWare.EpicOnlineServices;

namespace Core.Auth
{
    public interface IEOSWrapper
    {
        bool IsReady { get; }
        ProductUserId GetProductUserId();
        ConnectInterface GetEOSConnectInterface();
        void StartConnectLoginWithOptions(ExternalCredentialType credentialsType, string credentialsId, string displayName, System.Action<LoginCallbackInfo> callback);
        void ClearConnectId(ProductUserId localUserId);
    }

    public class EOSWrapper : IEOSWrapper
    {
        public bool IsReady => EOSManager.Instance != null;

        public ProductUserId GetProductUserId()
        {
            return EOSManager.Instance?.GetProductUserId();
        }

        public ConnectInterface GetEOSConnectInterface()
        {
            return EOSManager.Instance?.GetEOSConnectInterface();
        }

        public void StartConnectLoginWithOptions(ExternalCredentialType credentialsType, string credentialsId, string displayName, System.Action<LoginCallbackInfo> callback)
        {
            EOSManager.Instance?.StartConnectLoginWithOptions(credentialsType, credentialsId, displayName, (LoginCallbackInfo info) => callback?.Invoke(info));
        }

        public void ClearConnectId(ProductUserId localUserId)
        {
            EOSManager.Instance?.ClearConnectId(localUserId);
        }
    }
}
