using System.Collections.Generic;
using KitchenClash.Application.Models;
using NUnit.Framework;

namespace RecipeRage.Tests.EditMode.Gameplay.Persistence
{
    /// <summary>
    /// Unit tests for PlayerProgressData
    /// </summary>
    public class PlayerProgressDataTests
    {
        [Test]
        public void UnlockCharacter_AddsToUnlockedList()
        {
            var data = new PlayerProgressData();

            data.UnlockCharacter("Chef1");
            data.UnlockCharacter("Chef2");

            Assert.Contains("Chef1", data.UnlockedCharacters);
            Assert.Contains("Chef2", data.UnlockedCharacters);
        }

        [Test]
        public void UnlockCharacter_DoesNotDuplicate()
        {
            var data = new PlayerProgressData();

            data.UnlockCharacter("Chef1");
            data.UnlockCharacter("Chef1");

            Assert.AreEqual(1, data.UnlockedCharacters.Count);
        }

        [Test]
        public void UnlockMap_AddsToUnlockedList()
        {
            var data = new PlayerProgressData();

            data.UnlockMap("Kitchen");

            Assert.Contains("Kitchen", data.UnlockedMaps);
        }

        [Test]
        public void UnlockCosmetic_AddsToUnlockedList()
        {
            var data = new PlayerProgressData();

            data.UnlockCosmetic("Hat1");

            Assert.Contains("Hat1", data.UnlockedCosmetics);
        }

        [Test]
        public void UpdateHighScore_SetsNewHighScore()
        {
            var data = new PlayerProgressData();

            bool updated = data.UpdateHighScore("Classic", 5000);

            Assert.IsTrue(updated);
            Assert.AreEqual(5000, data.GameModeHighScores["Classic"]);
        }

        [Test]
        public void UpdateHighScore_DoesNotOverwriteHigherScore()
        {
            var data = new PlayerProgressData();

            data.UpdateHighScore("Classic", 5000);
            bool updated = data.UpdateHighScore("Classic", 3000);

            Assert.IsFalse(updated);
            Assert.AreEqual(5000, data.GameModeHighScores["Classic"]);
        }

        [Test]
        public void UpdateBestTime_SetsBestTime()
        {
            var data = new PlayerProgressData();

            bool updated = data.UpdateBestTime("SpeedRun", 120.5f);

            Assert.IsTrue(updated);
            Assert.AreEqual(120.5f, data.GameModeBestTimes["SpeedRun"]);
        }

        [Test]
        public void UpdateBestTime_DoesNotOverwriteBetterTime()
        {
            var data = new PlayerProgressData();

            data.UpdateBestTime("SpeedRun", 100f);
            bool updated = data.UpdateBestTime("SpeedRun", 120.5f);

            Assert.IsFalse(updated);
            Assert.AreEqual(100f, data.GameModeBestTimes["SpeedRun"]);
        }

        [Test]
        public void CharacterLevels_CanBeSetAndRetrieved()
        {
            var data = new PlayerProgressData();

            data.CharacterLevels["Chef1"] = 3;
            data.CharacterLevels["Chef2"] = 5;

            Assert.AreEqual(3, data.CharacterLevels["Chef1"]);
            Assert.AreEqual(5, data.CharacterLevels["Chef2"]);
        }

        [Test]
        public void AchievementProgress_CanBeTracked()
        {
            var data = new PlayerProgressData();

            data.CompletedAchievements.Add("FirstWin");
            data.AchievementProgress["Play100Games"] = 50;

            Assert.Contains("FirstWin", data.CompletedAchievements);
            Assert.AreEqual(50, data.AchievementProgress["Play100Games"]);
        }

        [Test]
        public void TutorialCompleted_DefaultsToFalse()
        {
            var data = new PlayerProgressData();

            Assert.IsFalse(data.TutorialCompleted);
        }

        [Test]
        public void TutorialCompleted_CanBeSet()
        {
            var data = new PlayerProgressData();

            data.TutorialCompleted = true;

            Assert.IsTrue(data.TutorialCompleted);
        }

        [Test]
        public void EosProductUserId_DefaultsToEmpty()
        {
            var data = new PlayerProgressData();

            Assert.AreEqual("", data.EosProductUserId);
        }

        [Test]
        public void EosProductUserId_CanBeSet()
        {
            var data = new PlayerProgressData();

            data.EosProductUserId = "linked-eos-id";

            Assert.AreEqual("linked-eos-id", data.EosProductUserId);
        }

        [Test]
        public void DataVersion_DefaultsToOne()
        {
            var data = new PlayerProgressData();

            Assert.AreEqual(1, data.DataVersion);
        }
    }
}
