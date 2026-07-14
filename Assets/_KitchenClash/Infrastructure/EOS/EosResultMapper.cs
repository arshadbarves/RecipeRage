using Epic.OnlineServices;
using KitchenClash.Application;

namespace KitchenClash.Infrastructure.EOS
{
    /// <summary>
    /// Maps vendor EOS Result codes to Application LobbyOpResult at the infrastructure boundary.
    /// </summary>
    internal static class EosResultMapper
    {
        public static LobbyOpResult ToLobbyOpResult(Result result)
        {
            if (result == Result.Success)
            {
                return LobbyOpResult.Ok();
            }

            return LobbyOpResult.Fail(result.ToString(), result.ToString());
        }
    }
}
