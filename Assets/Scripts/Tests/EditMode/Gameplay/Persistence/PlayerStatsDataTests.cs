using System;
using System.Collections.Generic;
using Gameplay.Persistence.Data;
using NUnit.Framework;

namespace RecipeRage.Tests.EditMode.Gameplay.Persistence
{
    /// <summary>
    /// Unit tests for PlayerStatsData account linking functionality (AC3)
    /// </summary>
    public class PlayerStatsDataTests
    {
        [Test]
        public void LinkToEosAccount_SetsEosProductUserId()
        {
            // Arrange
            var data = new PlayerStatsData();
            string eosId = "test-eos-id-123";

            // Act
            data.LinkToEosAccount(eosId, "Epic");

            // Assert
            Assert.AreEqual(eosId, data.EosProductUserId);
        }

        [Test]
        public void LinkToEosAccount_SetsLinkedAccountType()
        {
            // Arrange
            var data = new PlayerStatsData();

            // Act
            data.LinkToEosAccount("eos-id", "Steam");

            // Assert
            Assert.AreEqual("Steam", data.LinkedAccountType);
        }

        [Test]
        public void LinkToEosAccount_SetsLastLinkedAt()
        {
            // Arrange
            var data = new PlayerStatsData();
            var beforeLink = DateTime.UtcNow.AddSeconds(-1);

            // Act
            data.LinkToEosAccount("eos-id", "Epic");
            var afterLink = DateTime.UtcNow.AddSeconds(1);

            // Assert
            Assert.IsTrue(data.LastLinkedAt >= beforeLink && data.LastLinkedAt <= afterLink);
        }

        [Test]
        public void LinkToEosAccount_IncrementsLinkingVersion()
        {
            // Arrange
            var data = new PlayerStatsData();
            int initialVersion = data.AccountLinkingVersion;

            // Act
            data.LinkToEosAccount("eos-id", "Epic");

            // Assert
            Assert.AreEqual(initialVersion + 1, data.AccountLinkingVersion);
        }

        [Test]
        public void LinkToEosAccount_WithNullId_DoesNotModifyData()
        {
            // Arrange
            var data = new PlayerStatsData();
            data.EosProductUserId = "existing-id";

            // Act
            data.LinkToEosAccount(null, "Epic");

            // Assert
            Assert.AreEqual("existing-id", data.EosProductUserId);
        }

        [Test]
        public void IsLinkedToPermanentAccount_ReturnsFalse_WhenNotLinked()
        {
            // Arrange
            var data = new PlayerStatsData();

            // Assert
            Assert.IsFalse(data.IsLinkedToPermanentAccount);
        }

        [Test]
        public void IsLinkedToPermanentAccount_ReturnsTrue_WhenLinked()
        {
            // Arrange
            var data = new PlayerStatsData();
            data.LinkToEosAccount("eos-id", "Epic");

            // Assert
            Assert.IsTrue(data.IsLinkedToPermanentAccount);
        }

        [Test]
        public void CreateMigrationSnapshot_PreservesProgression()
        {
            // Arrange
            var data = new PlayerStatsData
            {
                Level = 5,
                Experience = 250,
                GamesPlayed = 10,
                GamesWon = 7,
                TotalScore = 5000
            };

            // Act
            var snapshot = data.CreateMigrationSnapshot();

            // Assert
            Assert.AreEqual(5, snapshot.Level);
            Assert.AreEqual(250, snapshot.Experience);
            Assert.AreEqual(10, snapshot.GamesPlayed);
            Assert.AreEqual(7, snapshot.GamesWon);
            Assert.AreEqual(5000, snapshot.TotalScore);
        }

        [Test]
        public void CreateMigrationSnapshot_PreservesCharacterUsage()
        {
            // Arrange
            var data = new PlayerStatsData();
            data.CharacterUsage["Chef1"] = 5;
            data.CharacterUsage["Chef2"] = 3;
            data.FavoriteCharacter = "Chef1";

            // Act
            var snapshot = data.CreateMigrationSnapshot();

            // Assert
            Assert.AreEqual(5, snapshot.CharacterUsage["Chef1"]);
            Assert.AreEqual(3, snapshot.CharacterUsage["Chef2"]);
            Assert.AreEqual("Chef1", snapshot.FavoriteCharacter);
        }

        [Test]
        public void CreateMigrationSnapshot_IncrementsLinkingVersion()
        {
            // Arrange
            var data = new PlayerStatsData();
            data.LinkToEosAccount("eos-id", "Epic");
            int versionBeforeSnapshot = data.AccountLinkingVersion;

            // Act
            var snapshot = data.CreateMigrationSnapshot();

            // Assert
            Assert.AreEqual(versionBeforeSnapshot + 1, snapshot.AccountLinkingVersion);
        }

        [Test]
        public void CreateMigrationSnapshot_SetsLastLinkedAt()
        {
            // Arrange
            var data = new PlayerStatsData();
            data.LinkToEosAccount("eos-id", "Epic");
            var beforeSnapshot = DateTime.UtcNow.AddSeconds(-1);

            // Act
            var snapshot = data.CreateMigrationSnapshot();
            var afterSnapshot = DateTime.UtcNow.AddSeconds(1);

            // Assert
            Assert.IsTrue(snapshot.LastLinkedAt >= beforeSnapshot && snapshot.LastLinkedAt <= afterSnapshot);
        }
    }
}
