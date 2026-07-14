using System;
using System.Collections.Generic;
using KitchenClash.Infrastructure.Logging;
using NUnit.Framework;
using Playcenter.Shell;

namespace RecipeRage.Tests.EditMode.Gameplay
{
    public class LoggingBootstrapTests
    {
        private sealed class RecordingLoggingService : ILoggingService
        {
            public readonly List<(string Message, LogLevel Level, string Category)> Entries = new();

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

            public void LogException(Exception exception, string category = "General") =>
                Log(exception.Message, LogLevel.Error, category);

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
        public void Initialize_WiresGameLogger_AndEmitsBootstrapLine()
        {
            var recording = new RecordingLoggingService();
            var bootstrap = new LoggingBootstrap(recording);

            Assert.That(GameLogger.IsWired, Is.False);

            bootstrap.Initialize();

            Assert.That(GameLogger.IsWired, Is.True);
            Assert.That(recording.Entries, Has.Count.EqualTo(1));
            Assert.That(recording.Entries[0].Message, Does.Contain("LoggingBootstrap"));
            Assert.That(recording.Entries[0].Category, Is.EqualTo("Logging"));

            GameLogger.Log("product");
            Assert.That(recording.Entries, Has.Count.EqualTo(2));
            Assert.That(recording.Entries[1].Message, Is.EqualTo("product"));
        }

        [Test]
        public void Initialize_IsIdempotent_WhenAlreadyWired()
        {
            var first = new RecordingLoggingService();
            var second = new RecordingLoggingService();
            GameLogger.SetService(first);

            new LoggingBootstrap(second).Initialize();

            GameLogger.Log("after");
            Assert.That(first.Entries, Is.Empty);
            Assert.That(second.Entries, Has.Count.EqualTo(2)); // bootstrap line + product
            Assert.That(second.Entries[1].Message, Is.EqualTo("after"));
        }
    }
}
