using Epic.OnlineServices;
using Epic.OnlineServices.Connect;
using PlayEveryWare.EpicOnlineServices;

namespace Core.Authentication
{
    public interface IEOSWrapper
    {
        bool IsReady { get; }
        ProductUserId GetProductUserId();
        ConnectInterface GetEOSConnectInterface();
        void StartConnectLoginWithOptions(ExternalCredentialType credentialsType, string credentialsId, string displayName, OnLoginCallback callback);
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

        public void StartConnectLoginWithOptions(ExternalCredentialType credentialsType, string credentialsId, string displayName, OnLoginCallback callback)
        {
            EOSManager.Instance?.StartConnectLoginWithOptions(credentialsType, credentialsId, displayName, callback);
        }

        public void ClearConnectId(ProductUserId localUserId)
        {
            EOSManager.Instance?.ClearConnectId(localUserId);
        }
    }
}
