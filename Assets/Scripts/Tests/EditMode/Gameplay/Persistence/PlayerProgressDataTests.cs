using System.Collections.Generic;
using Gameplay.Persistence.Data;
using NUnit.Framework;

namespace RecipeRage.Tests.EditMode.Gameplay.Persistence
{
    /// <summary>
    /// Unit tests for PlayerProgressData account linking functionality (AC3)
    /// </summary>
    public class PlayerProgressDataTests
    {
        [Test]
        public void LinkToEosAccount_SetsEosProductUserId()
        {
            // Arrange
            var data = new PlayerProgressData();
            string eosId = "test-eos-id-456";

            // Act
            data.LinkToEosAccount(eosId);

            // Assert
            Assert.AreEqual(eosId, data.EosProductUserId);
        }

        [Test]
        public void LinkToEosAccount_IncrementsDataVersion()
        {
            // Arrange
            var data = new PlayerProgressData();
            int initialVersion = data.DataVersion;

            // Act
            data.LinkToEosAccount("eos-id");

            // Assert
            Assert.AreEqual(initialVersion + 1, data.DataVersion);
        }

        [Test]
        public void LinkToEosAccount_WithNullId_DoesNotModifyData()
        {
            // Arrange
            var data = new PlayerProgressData();
            data.EosProductUserId = "existing-id";

            // Act
            data.LinkToEosAccount(null);

            // Assert
            Assert.AreEqual("existing-id", data.EosProductUserId);
        }

        [Test]
        public void CreateMigrationSnapshot_PreservesUnlockedContent()
        {
            // Arrange
            var data = new PlayerProgressData();
            data.UnlockCharacter("Chef1");
            data.UnlockCharacter("Chef2");
            data.UnlockMap("Kitchen");
            data.UnlockCosmetic("Hat1");

            // Act
            var snapshot = data.CreateMigrationSnapshot();

            // Assert
            Assert.IsTrue(snapshot.IsCharacterUnlocked("Chef1"));
            Assert.IsTrue(snapshot.IsCharacterUnlocked("Chef2"));
            Assert.IsTrue(snapshot.IsMapUnlocked("Kitchen"));
            Assert.IsTrue(snapshot.IsCosmeticUnlocked("Hat1"));
        }

        [Test]
        public void CreateMigrationSnapshot_PreservesHighScores()
        {
            // Arrange
            var data = new PlayerProgressData();
            data.UpdateHighScore("Classic", 5000);
            data.UpdateHighScore("TimeAttack", 3000);

            // Act
            var snapshot = data.CreateMigrationSnapshot();

            // Assert
            Assert.AreEqual(5000, snapshot.GameModeHighScores["Classic"]);
            Assert.AreEqual(3000, snapshot.GameModeHighScores["TimeAttack"]);
        }

        [Test]
        public void CreateMigrationSnapshot_PreservesBestTimes()
        {
            // Arrange
            var data = new PlayerProgressData();
            data.UpdateBestTime("SpeedRun", 120.5f);

            // Act
            var snapshot = data.CreateMigrationSnapshot();

            // Assert
            Assert.AreEqual(120.5f, snapshot.GameModeBestTimes["SpeedRun"]);
        }

        [Test]
        public void CreateMigrationSnapshot_PreservesCharacterLevels()
        {
            // Arrange
            var data = new PlayerProgressData();
            data.CharacterLevels["Chef1"] = 3;
            data.CharacterLevels["Chef2"] = 5;

            // Act
            var snapshot = data.CreateMigrationSnapshot();

            // Assert
            Assert.AreEqual(3, snapshot.CharacterLevels["Chef1"]);
            Assert.AreEqual(5, snapshot.CharacterLevels["Chef2"]);
        }

        [Test]
        public void CreateMigrationSnapshot_PreservesAchievementProgress()
        {
            // Arrange
            var data = new PlayerProgressData();
            data.CompletedAchievements.Add("FirstWin");
            data.AchievementProgress["Play100Games"] = 50;

            // Act
            var snapshot = data.CreateMigrationSnapshot();

            // Assert
            Assert.Contains("FirstWin", snapshot.CompletedAchievements);
            Assert.AreEqual(50, snapshot.AchievementProgress["Play100Games"]);
        }

        [Test]
        public void CreateMigrationSnapshot_PreservesTutorialStatus()
        {
            // Arrange
            var data = new PlayerProgressData();
            data.TutorialCompleted = true;

            // Act
            var snapshot = data.CreateMigrationSnapshot();

            // Assert
            Assert.IsTrue(snapshot.TutorialCompleted);
        }

        [Test]
        public void CreateMigrationSnapshot_IncrementsDataVersion()
        {
            // Arrange
            var data = new PlayerProgressData();
            data.LinkToEosAccount("eos-id");
            int versionBeforeSnapshot = data.DataVersion;

            // Act
            var snapshot = data.CreateMigrationSnapshot();

            // Assert
            Assert.AreEqual(versionBeforeSnapshot + 1, snapshot.DataVersion);
        }

        [Test]
        public void CreateMigrationSnapshot_PreservesEosProductUserId()
        {
            // Arrange
            var data = new PlayerProgressData();
            data.LinkToEosAccount("linked-eos-id");

            // Act
            var snapshot = data.CreateMigrationSnapshot();

            // Assert
            Assert.AreEqual("linked-eos-id", snapshot.EosProductUserId);
        }
    }
}
