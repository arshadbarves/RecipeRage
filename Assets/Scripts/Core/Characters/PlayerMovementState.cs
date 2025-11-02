namespace Core.Characters
{
    /// <summary>
    /// Defines the possible movement states for a player character.
    /// Used for state machine-based movement control.
    /// </summary>
    public enum PlayerMovementState
    {
        /// <summary>
        /// Player is standing still with no input.
        /// </summary>
        Idle,

        /// <summary>
        /// Player is moving normally.
        /// </summary>
        Moving,

        /// <summary>
        /// Player is interacting with a station or object.
        /// Movement may be restricted or disabled.
        /// </summary>
        Interacting,

        /// <summary>
        /// Player is using their special ability.
        /// Movement may be modified or disabled.
        /// </summary>
        UsingAbility,

        /// <summary>
        /// Player is stunned or disabled.
        /// No movement allowed.
        /// </summary>
        Stunned,

        /// <summary>
        /// Player is carrying an object.
        /// Movement speed may be reduced.
        /// </summary>
        Carrying
    }
}
