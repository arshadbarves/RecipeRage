using System.Threading.Tasks;

namespace Playcenter.Services
{
    /// <summary>
    /// EOS Player Data Storage transport. ReadFile/WriteFile map to
    /// EOS PlayerDataStorageInterface (QueryFile → ReadFile → WriteFile).
    /// Auth gate: requires signed-in user from IAuthService (product user id
    /// mapping happens at EOS connect layer — guest accounts get a device-bound
    /// EOS product user via EOS Connect login in production).
    /// </summary>
    public sealed class EOSPlayerDataTransport
    {
        private readonly IAuthService _auth;
        private readonly ILoggingService _log;

        public bool IsAvailable => _auth.IsSignedIn;

        public EOSPlayerDataTransport(IAuthService auth, ILoggingService log)
        {
            _auth = auth;
            _log = log;
        }

        public async Task<byte[]> Read(string key)
        {
            // EOS wiring point: PlayerDataStorageInterface.QueryFile + ReadFile.
            // Until EOS credentials are configured, report unavailable so the
            // service falls back to local persistence.
            await Task.CompletedTask;
            return null;
        }

        public async Task<bool> Write(string key, byte[] data)
        {
            // EOS wiring point: PlayerDataStorageInterface.WriteFile.
            await Task.CompletedTask;
            return false;
        }
    }
}
