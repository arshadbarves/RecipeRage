using System;
using KitchenClash.Application.Models;
using NUnit.Framework;

namespace RecipeRage.Tests.EditMode.Gameplay.Persistence
{
    /// <summary>
    /// Unit tests for PlayerStatsData
    /// </summary>
    public class PlayerStatsDataTests
    {
        [Test]
        public void NewPlayerStatsData_HasDefaultValues()
        {
            var data = new PlayerStatsData();

            Assert.AreEqual(1, data.Level);
            Assert.AreEqual(0, data.Experience);
            Assert.AreEqual(0, data.GamesPlayed);
            Assert.AreEqual(0, data.GamesWon);
            Assert.AreEqual(0, data.TotalScore);
        }

        [Test]
        public void AddExperience_IncreasesExperience()
        {
            var data = new PlayerStatsData();

            data.AddExperience(50);

            Assert.AreEqual(50, data.Experience);
        }

        [Test]
        public void AddExperience_LevelsUp_WhenThresholdReached()
        {
            var data = new PlayerStatsData();

            bool leveledUp = data.AddExperience(100);

            Assert.IsTrue(leveledUp);
            Assert.AreEqual(2, data.Level);
            Assert.AreEqual(0, data.Experience);
        }

        [Test]
        public void AddExperience_DoesNotLevelUp_WhenBelowThreshold()
        {
            var data = new PlayerStatsData();

            bool leveledUp = data.AddExperience(50);

            Assert.IsFalse(leveledUp);
            Assert.AreEqual(1, data.Level);
        }

        [Test]
        public void RecordGamePlayed_IncrementsGamesPlayed()
        {
            var data = new PlayerStatsData();

            data.RecordGamePlayed(true, "classic", "Chef1", 120f, 500);

            Assert.AreEqual(1, data.GamesPlayed);
            Assert.AreEqual(1, data.GamesWon);
            Assert.AreEqual(0, data.GamesLost);
            Assert.AreEqual(500, data.TotalScore);
        }

        [Test]
        public void RecordGamePlayed_TracksCharacterUsage()
        {
            var data = new PlayerStatsData();

            data.RecordGamePlayed(true, "classic", "Chef1", 60f, 100);
            data.RecordGamePlayed(false, "classic", "Chef1", 60f, 50);
            data.RecordGamePlayed(true, "classic", "Chef2", 60f, 200);

            Assert.AreEqual(2, data.CharacterUsage["Chef1"]);
            Assert.AreEqual(1, data.CharacterUsage["Chef2"]);
        }

        [Test]
        public void RecordGamePlayed_TracksGameModeUsage()
        {
            var data = new PlayerStatsData();

            data.RecordGamePlayed(true, "classic", "Chef1", 60f, 100);
            data.RecordGamePlayed(true, "ranked", "Chef1", 60f, 200);

            Assert.AreEqual(1, data.GameModeUsage["classic"]);
            Assert.AreEqual(1, data.GameModeUsage["ranked"]);
        }

        [Test]
        public void RecordGamePlayed_Loss_IncrementsGamesLost()
        {
            var data = new PlayerStatsData();

            data.RecordGamePlayed(false, "classic", "Chef1", 60f, 50);

            Assert.AreEqual(1, data.GamesPlayed);
            Assert.AreEqual(0, data.GamesWon);
            Assert.AreEqual(1, data.GamesLost);
        }

        [Test]
        public void EosProductUserId_DefaultsToEmpty()
        {
            var data = new PlayerStatsData();

            Assert.AreEqual("", data.EosProductUserId);
        }

        [Test]
        public void EosProductUserId_CanBeSet()
        {
            var data = new PlayerStatsData();

            data.EosProductUserId = "test-eos-id-123";

            Assert.AreEqual("test-eos-id-123", data.EosProductUserId);
        }
    }
}
