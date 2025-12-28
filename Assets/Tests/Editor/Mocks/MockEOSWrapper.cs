using Core.Authentication;
using Epic.OnlineServices;
using Epic.OnlineServices.Connect;

namespace Tests.Editor.Mocks
{
    public class MockEOSWrapper : IEOSWrapper
    {
        public bool IsReady { get; set; } = true;
        
        public ProductUserId GetProductUserId() => null;
        public ConnectInterface GetEOSConnectInterface() => null;
        public void StartConnectLoginWithOptions(ExternalCredentialType credentialsType, string credentialsId, string displayName, System.Action<LoginCallbackInfo> callback) { }
        public void ClearConnectId(ProductUserId localUserId) { }
    }
}
