using System.Collections;
using System.Threading.Tasks;
using KitchenClash.Domain;

namespace KitchenClash.Infrastructure.Async
{
    public static class TaskExtensions
    {
        public static IEnumerator AsCoroutine(this Task task)
        {
            while (!task.IsCompleted)
            {
                yield return null;
            }

            if (task.IsFaulted)
            {
                GameLogger.LogError($"Task failed: {task.Exception}");
            }
        }

        public static IEnumerator AsCoroutine<T>(this Task<T> task)
        {
            while (!task.IsCompleted)
            {
                yield return null;
            }

            if (task.IsFaulted)
            {
                GameLogger.LogError($"Task failed: {task.Exception}");
            }
        }
    }
}
