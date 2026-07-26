using Epic.OnlineServices;

namespace Playcenter.EOS
{
    /// <summary>
    /// Generic EOS Result helpers — no title-specific result types.
    /// </summary>
    public static class EosResultMapper
    {
        public static bool IsSuccess(Result result) => result == Result.Success;

        public static string ToErrorCode(Result result) => result.ToString();
    }
}
