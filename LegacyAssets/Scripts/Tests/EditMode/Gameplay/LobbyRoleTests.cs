using NUnit.Framework;
using Playcenter.Services;
using System;
using System.Collections.Generic;

namespace KitchenClash.Tests.EditMode.Gameplay
{
    /// <summary>
    /// In-memory ILobbyManager for EditMode tests.
    /// Tracks party and match lobbies independently (Brawl/PUBG style).
    /// </summary>
    public sealed class FakeLobbyManager : ILobbyManager
    {
        private LobbyInfo _party;
        private LobbyInfo _match;

        public event Action<LobbyOpResult, LobbyInfo> OnMatchLobbyCreated;
        public event Action<LobbyOpResult, LobbyInfo> OnMatchLobbyJoined;
        public event Action OnMatchLobbyLeft;
        public event Action OnMatchLobbyUpdated;
        public event Action<LobbyState> OnLobbyStateChanged;
        public event Action<string> OnError;

        public LobbyInfo CurrentPartyLobby => _party;
        public LobbyInfo CurrentMatchLobby => _match;
        public LobbyState CurrentState { get; private set; }
        public bool IsInParty => _party != null;
        public bool IsInMatchLobby => _match != null;
        public bool IsPartyLeader => true;
        public bool IsMatchLobbyOwner => true;

        public void Initialize() { }
        public void Dispose() { }

        /// <summary>Test convenience: create party or match lobby by type.</summary>
        public void Create(LobbyType type)
        {
            if (type == LobbyType.Party)
            {
                CreatePartyLobby(new LobbyConfig { Type = LobbyType.Party, LobbyName = "party" });
            }
            else
            {
                CreateMatchLobby(new LobbyConfig { Type = LobbyType.Match, LobbyName = "match" });
            }
        }

        public bool Has(LobbyType type) =>
            type == LobbyType.Party ? _party != null : _match != null;

        public void Destroy(LobbyType type)
        {
            if (type == LobbyType.Match)
            {
                DestroyMatchLobby();
            }
            else
            {
                LeaveParty();
            }
        }

        public void CreatePartyLobby(LobbyConfig config)
        {
            _party = new LobbyInfo
            {
                LobbyId = "party-1",
                Type = LobbyType.Party,
                LobbyName = config?.LobbyName ?? "party",
                MaxPlayers = 3,
                CurrentPlayers = 1
            };
            CurrentState = LobbyState.InParty;
            OnLobbyStateChanged?.Invoke(LobbyState.InParty);
        }

        public void InviteToParty(string friendProductUserId) { }

        public void LeaveParty()
        {
            _party = null;
            CurrentState = IsInMatchLobby ? LobbyState.InMatchLobby : LobbyState.Idle;
            OnLobbyStateChanged?.Invoke(CurrentState);
        }

        public void UpdatePartySettings(LobbyConfig config) { }

        public void CreateMatchLobby(LobbyConfig config)
        {
            _match = new LobbyInfo
            {
                LobbyId = "match-1",
                Type = LobbyType.Match,
                LobbyName = config?.LobbyName ?? "match",
                MaxPlayers = 6,
                CurrentPlayers = 1
            };
            CurrentState = LobbyState.InMatchLobby;
            OnMatchLobbyCreated?.Invoke(LobbyOpResult.Ok(), _match);
            OnLobbyStateChanged?.Invoke(LobbyState.InMatchLobby);
        }

        public void JoinMatchLobby(string lobbyId)
        {
            _match = new LobbyInfo
            {
                LobbyId = lobbyId,
                Type = LobbyType.Match,
                MaxPlayers = 6,
                CurrentPlayers = 2
            };
            CurrentState = LobbyState.InMatchLobby;
            OnMatchLobbyJoined?.Invoke(LobbyOpResult.Ok(), _match);
            OnLobbyStateChanged?.Invoke(LobbyState.InMatchLobby);
        }

        public void LeaveMatchLobby()
        {
            _match = null;
            CurrentState = IsInParty ? LobbyState.InParty : LobbyState.Idle;
            OnMatchLobbyLeft?.Invoke();
            OnLobbyStateChanged?.Invoke(CurrentState);
        }

        public void DestroyMatchLobby()
        {
            _match = null;
            CurrentState = IsInParty ? LobbyState.InParty : LobbyState.Idle;
            OnMatchLobbyLeft?.Invoke();
            OnLobbyStateChanged?.Invoke(CurrentState);
        }

        public void SetGameMode(string gameModeId) { }
        public void SetMapName(string mapName) { }
        public bool AreAllPlayersReady() => true;

        public LobbyInfo GetLobbyInfo(string lobbyId)
        {
            if (_party?.LobbyId == lobbyId)
            {
                return _party;
            }

            if (_match?.LobbyId == lobbyId)
            {
                return _match;
            }

            return null;
        }
    }

    [TestFixture]
    public class LobbyRoleTests
    {
        [Test]
        public void DestroyMatchLobby_KeepsParty()
        {
            var fake = new FakeLobbyManager();
            fake.Create(LobbyType.Party);
            fake.Create(LobbyType.Match);

            fake.Destroy(LobbyType.Match);

            Assert.IsTrue(fake.Has(LobbyType.Party));
            Assert.IsFalse(fake.Has(LobbyType.Match));
        }

        [Test]
        public void LeaveMatchLobby_DoesNotClearParty()
        {
            var fake = new FakeLobbyManager();
            fake.CreatePartyLobby(new LobbyConfig());
            fake.CreateMatchLobby(new LobbyConfig());

            fake.LeaveMatchLobby();

            Assert.IsTrue(fake.IsInParty);
            Assert.IsFalse(fake.IsInMatchLobby);
        }

        [Test]
        public void CreatePartyThenMatch_BothTrackedIndependently()
        {
            var fake = new FakeLobbyManager();
            fake.CreatePartyLobby(new LobbyConfig { LobbyName = "p" });
            fake.CreateMatchLobby(new LobbyConfig { LobbyName = "m" });

            Assert.AreEqual("party-1", fake.CurrentPartyLobby.LobbyId);
            Assert.AreEqual("match-1", fake.CurrentMatchLobby.LobbyId);
            Assert.AreNotEqual(fake.CurrentPartyLobby.LobbyId, fake.CurrentMatchLobby.LobbyId);
        }
    }
}
