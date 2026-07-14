using System;
using System.Collections.Generic;
using NUnit.Framework;
using Playcenter.Shell;

namespace RecipeRage.Tests.EditMode.Gameplay
{
    public class GameLoggerTests
    {
        private sealed class RecordingLoggingService : ILoggingService
        {
            public readonly List<(string Message, LogLevel Level, string Category)> Entries = new();
            public readonly List<Exception> Exceptions = new();

            public event Action<LogEntry> OnLogAdded;

            public void Log(string message, LogLevel level = LogLevel.Info, string category = "General")
            {
                Entries.Add((message, level, category));
                OnLogAdded?.Invoke(new LogEntry(message, level, category));
            }

            public void LogInfo(string message, string category = "General") =>
                Log(message, LogLevel.Info, category);

            public void LogWarning(string message, string category = "General") =>
                Log(message, LogLevel.Warning, category);

            public void LogError(string message, string category = "General") =>
                Log(message, LogLevel.Error, category);

            public void LogException(Exception exception, string category = "General")
            {
                Exceptions.Add(exception);
                Entries.Add((exception.Message, LogLevel.Error, category));
            }

            public LogEntry[] GetLogs() => Array.Empty<LogEntry>();
            public void ClearLogs() { }
            public void SaveLogsToFile(string filePath) { }
            public void Dispose() { }
        }

        [SetUp]
        public void SetUp() => GameLogger.ClearService();

        [TearDown]
        public void TearDown() => GameLogger.ClearService();

        [Test]
        public void Log_WithoutService_Throws()
        {
            Assert.That(GameLogger.IsWired, Is.False);
            Assert.Throws<InvalidOperationException>(() => GameLogger.Log("orphan"));
        }

        [Test]
        public void LogInfo_WithoutService_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => GameLogger.LogInfo("orphan"));
        }

        [Test]
        public void LogError_WithoutService_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => GameLogger.LogError("orphan"));
        }

        [Test]
        public void SetService_ThenLog_RoutesToService()
        {
            var recording = new RecordingLoggingService();
            GameLogger.SetService(recording);

            Assert.That(GameLogger.IsWired, Is.True);

            GameLogger.Log("hello");
            GameLogger.LogInfo("info");
            GameLogger.LogWarning("warn");
            GameLogger.LogError("err");

            Assert.That(recording.Entries.Count, Is.EqualTo(4));
            Assert.That(recording.Entries[0].Message, Is.EqualTo("hello"));
            Assert.That(recording.Entries[1].Message, Is.EqualTo("info"));
            Assert.That(recording.Entries[1].Level, Is.EqualTo(LogLevel.Info));
            Assert.That(recording.Entries[2].Level, Is.EqualTo(LogLevel.Warning));
            Assert.That(recording.Entries[3].Level, Is.EqualTo(LogLevel.Error));
        }

        [Test]
        public void LogException_RoutesToService()
        {
            var recording = new RecordingLoggingService();
            GameLogger.SetService(recording);
            var ex = new InvalidOperationException("boom");

            GameLogger.LogException(ex);

            Assert.That(recording.Exceptions, Has.Count.EqualTo(1));
            Assert.That(recording.Exceptions[0], Is.SameAs(ex));
        }

        [Test]
        public void ClearService_UnwiresFacade()
        {
            GameLogger.SetService(new RecordingLoggingService());
            Assert.That(GameLogger.IsWired, Is.True);

            GameLogger.ClearService();

            Assert.That(GameLogger.IsWired, Is.False);
            Assert.Throws<InvalidOperationException>(() => GameLogger.Log("after clear"));
        }
    }
}
