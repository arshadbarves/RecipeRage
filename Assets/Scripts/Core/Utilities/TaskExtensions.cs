using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace Core.Utilities
{
    /// <summary>
    /// Extension methods for Task to work with Unity coroutines.
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// Convert a Task to a coroutine.
        /// </summary>
        /// <param name="task">The task to convert</param>
        /// <returns>A coroutine that waits for the task to complete</returns>
        public static IEnumerator AsCoroutine(this Task task)
        {
            while (!task.IsCompleted)
            {
                yield return null;
            }
            
            if (task.IsFaulted)
            {
                Debug.LogError($"Task failed with exception: {task.Exception}");
            }
        }
        
        /// <summary>
        /// Convert a Task<T> to a coroutine.
        /// </summary>
        /// <typeparam name="T">The type of the task result</typeparam>
        /// <param name="task">The task to convert</param>
        /// <returns>A coroutine that waits for the task to complete</returns>
        public static IEnumerator AsCoroutine<T>(this Task<T> task)
        {
            while (!task.IsCompleted)
            {
                yield return null;
            }
            
            if (task.IsFaulted)
            {
                Debug.LogError($"Task failed with exception: {task.Exception}");
            }
        }
    }
}
