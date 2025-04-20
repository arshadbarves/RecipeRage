using UnityEngine;

namespace Core.GameFramework.State
{
    /// <summary>
    /// Utility class for state-related operations.
    /// </summary>
    public static class StateUtility
    {
        /// <summary>
        /// Log a state entry message.
        /// </summary>
        /// <param name="stateName">Name of the state</param>
        public static void LogStateEnter(string stateName)
        {
            Debug.Log($"[{stateName}] Entered");
        }
        
        /// <summary>
        /// Log a state exit message.
        /// </summary>
        /// <param name="stateName">Name of the state</param>
        public static void LogStateExit(string stateName)
        {
            Debug.Log($"[{stateName}] Exited");
        }
        
        /// <summary>
        /// Log a state transition message.
        /// </summary>
        /// <param name="fromState">Name of the source state</param>
        /// <param name="toState">Name of the target state</param>
        public static void LogStateTransition(string fromState, string toState)
        {
            Debug.Log($"State transition: {fromState} -> {toState}");
        }
        
        /// <summary>
        /// Log a state action message.
        /// </summary>
        /// <param name="stateName">Name of the state</param>
        /// <param name="action">Action being performed</param>
        public static void LogStateAction(string stateName, string action)
        {
            Debug.Log($"[{stateName}] {action}");
        }
    }
}
