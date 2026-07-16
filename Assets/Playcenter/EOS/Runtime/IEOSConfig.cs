namespace Playcenter.EOS
{
    /// <summary>
    /// Title-supplied EOS/UGS identifiers. Game maps ScriptableObject → this.
    /// </summary>
    public interface IEOSConfig
    {
        /// <summary>Unity Gaming Services project id (if UGS bridge is used).</summary>
        string UgsProjectId { get; }

        /// <summary>UGS authentication profile name (InitializationOptions.SetProfile).</summary>
        string AuthenticationProfile { get; }

        /// <summary>When false, skip UGS authentication bridge.</summary>
        bool EnableUgsBridge { get; }
    }
}
