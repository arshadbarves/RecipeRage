using System.Collections.Generic;
using System.Threading.Tasks;
using KitchenClash.Application;
using KitchenClash.Domain;
using NUnit.Framework;

namespace RecipeRage.Tests.EditMode.Gameplay
{
    public class MatchServiceTests
    {
        [Test]
        public void GetQueues_ReturnsKitchenClashLaunchQueuesByDefault()
        {
            MatchService service = new(new DictionaryConfigService());

            IReadOnlyList<MatchQueueDefinition> queues = service.GetQueues();

            Assert.AreEqual(3, queues.Count);
            Assert.AreEqual("quick_2v2", queues[0].ModeId);
            Assert.AreEqual("quick_3v3", queues[1].ModeId);
            Assert.AreEqual("ranked", queues[2].ModeId);
        }

        [Test]
        public void TryGetQueue_FindsExistingQueue()
        {
            DictionaryConfigService config = new();

            MatchService service = new(config);

            Assert.IsTrue(service.TryGetQueue("quick_2v2", out MatchQueueDefinition queue));
            Assert.AreEqual("quick_2v2", queue.ModeId);
        }

        [Test]
        public void TryGetQueue_ReturnsFalseForUnknownQueue()
        {
            MatchService service = new(new DictionaryConfigService());

            Assert.IsFalse(service.TryGetQueue("nonexistent", out _));
        }

        private sealed class DictionaryConfigService : IConfigService
        {
            private readonly Dictionary<string, object> _values = new();

            public void Set(string key, object value)
            {
                _values[key] = value;
            }

            public T Get<T>(string key, T fallback)
            {
                if (_values.TryGetValue(key, out object value))
                {
                    try { return (T)System.Convert.ChangeType(value, typeof(T)); }
                    catch { return fallback; }
                }
                return fallback;
            }

            public Task FetchAsync() => Task.CompletedTask;
        }
    }
}
