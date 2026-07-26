using KitchenClash.Application;
using PlayEveryWare.EpicOnlineServices;

namespace KitchenClash.Infrastructure.EOS
{
    /// <summary>
    /// Reads the local EOS Product User Id as a platform-neutral string.
    /// </summary>
    public sealed class EOSLocalNetworkIdentity : ILocalNetworkIdentity
    {
        public string LocalUserId => EOSManager.Instance?.GetProductUserId()?.ToString();
    }
}
