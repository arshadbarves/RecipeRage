namespace RecipeRage.Modules.Lobbies.Data
{
    /// <summary>
    /// Defines permission levels for lobby access
    /// </summary>
    public enum LobbyPermission
    {
        /// <summary>
        /// Anyone can join without restrictions
        /// </summary>
        Public,

        /// <summary>
        /// Only friends of current members can join
        /// </summary>
        FriendsOnly,

        /// <summary>
        /// Only those with an invite can join
        /// </summary>
        InviteOnly,

        /// <summary>
        /// Lobby is locked and cannot be joined
        /// </summary>
        Locked,

        /// <summary>
        /// Only the owner can invite players to join
        /// </summary>
        OwnerInviteOnly,

        /// <summary>
        /// Players must be approved by the owner to join
        /// </summary>
        ApprovalRequired,

        /// <summary>
        /// Only players who pass a certain criteria can join
        /// </summary>
        RulesBased
    }
}