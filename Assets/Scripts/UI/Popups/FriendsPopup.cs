using System.Collections.Generic;
using Core.Bootstrap;
using UI.Core;
using UI.Screens;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Popups
{
    /// <summary>
    /// Friends popup - shows friends list and party management
    /// </summary>
    [UIScreen(UIScreenType.Popup, UIScreenPriority.Popup, "FriendsPopupTemplate")]
    public class FriendsPopup : BaseUIScreen
    {
        // UI Elements
        private Button _closeButton;
        private ScrollView _friendsList;
        private Label _titleLabel;
        private VisualElement _partySection;
        private ScrollView _partyList;
        
        protected override void OnInitialize()
        {
            CacheUIElements();
            SetupButtons();
            
            Debug.Log("[FriendsPopup] Initialized");
        }
        
        private void CacheUIElements()
        {
            _closeButton = GetElement<Button>("close-button");
            _friendsList = GetElement<ScrollView>("friends-list");
            _titleLabel = GetElement<Label>("title-label");
            _partySection = GetElement<VisualElement>("party-section");
            _partyList = GetElement<ScrollView>("party-list");
            
            if (_titleLabel != null)
            {
                _titleLabel.text = "FRIENDS & PARTY";
            }
        }
        
        private void SetupButtons()
        {
            if (_closeButton != null)
            {
                _closeButton.clicked += OnCloseClicked;
            }
        }
        
        protected override void OnShow()
        {
            LoadFriends();
            LoadParty();
        }
        
        private void LoadFriends()
        {
            if (_friendsList == null) return;
            
            _friendsList.Clear();
            
            // TODO: Get friends from EOS Friends service
            // For now, show placeholder
            List<FriendInfo> friends = GetMockFriends();
            
            foreach (FriendInfo friend in friends)
            {
                VisualElement friendEntry = CreateFriendEntry(friend);
                _friendsList.Add(friendEntry);
            }
            
            Debug.Log($"[FriendsPopup] Loaded {friends.Count} friends");
        }
        
        private void LoadParty()
        {
            if (_partyList == null) return;
            
            _partyList.Clear();
            
            // TODO: Get party members from network manager
            // For now, show placeholder
            List<PartyMember> partyMembers = GetMockPartyMembers();
            
            foreach (PartyMember member in partyMembers)
            {
                VisualElement memberEntry = CreatePartyMemberEntry(member);
                _partyList.Add(memberEntry);
            }
            
            Debug.Log($"[FriendsPopup] Loaded {partyMembers.Count} party members");
        }
        
        private VisualElement CreateFriendEntry(FriendInfo friend)
        {
            VisualElement entry = new VisualElement();
            entry.AddToClassList("friend-entry");
            
            // Avatar
            VisualElement avatar = new VisualElement();
            avatar.AddToClassList("friend-avatar");
            entry.Add(avatar);
            
            // Info
            VisualElement info = new VisualElement();
            info.AddToClassList("friend-info");
            
            Label nameLabel = new Label(friend.displayName);
            nameLabel.AddToClassList("friend-name");
            info.Add(nameLabel);
            
            Label statusLabel = new Label(friend.isOnline ? "Online" : "Offline");
            statusLabel.AddToClassList("friend-status");
            statusLabel.AddToClassList(friend.isOnline ? "online" : "offline");
            info.Add(statusLabel);
            
            entry.Add(info);
            
            // Invite button (only if online)
            if (friend.isOnline)
            {
                Button inviteButton = new Button(() => OnInviteFriend(friend));
                inviteButton.text = "INVITE";
                inviteButton.AddToClassList("invite-button");
                entry.Add(inviteButton);
            }
            
            return entry;
        }
        
        private VisualElement CreatePartyMemberEntry(PartyMember member)
        {
            VisualElement entry = new VisualElement();
            entry.AddToClassList("party-member-entry");
            
            // Avatar
            VisualElement avatar = new VisualElement();
            avatar.AddToClassList("party-avatar");
            entry.Add(avatar);
            
            // Info
            VisualElement info = new VisualElement();
            info.AddToClassList("party-info");
            
            Label nameLabel = new Label(member.displayName);
            nameLabel.AddToClassList("party-name");
            
            if (member.isLeader)
            {
                nameLabel.text += " (Leader)";
            }
            
            info.Add(nameLabel);
            
            Label readyLabel = new Label(member.isReady ? "Ready" : "Not Ready");
            readyLabel.AddToClassList("party-ready");
            readyLabel.AddToClassList(member.isReady ? "ready" : "not-ready");
            info.Add(readyLabel);
            
            entry.Add(info);
            
            // Kick button (only if you're leader and not yourself)
            if (member.canKick && !member.isLocal)
            {
                Button kickButton = new Button(() => OnKickMember(member));
                kickButton.text = "KICK";
                kickButton.AddToClassList("kick-button");
                entry.Add(kickButton);
            }
            
            return entry;
        }
        
        private void OnInviteFriend(FriendInfo friend)
        {
            Debug.Log($"[FriendsPopup] Inviting friend: {friend.displayName}");
            
            // TODO: Send party invite via EOS
            var uiService = GameBootstrap.Services?.UIService;
            uiService?.ShowToast($"Invited {friend.displayName} to party", ToastType.Info, 2f);
        }
        
        private void OnKickMember(PartyMember member)
        {
            Debug.Log($"[FriendsPopup] Kicking member: {member.displayName}");
            
            // TODO: Kick from party via network manager
            var uiService = GameBootstrap.Services?.UIService;
            uiService?.ShowToast($"Kicked {member.displayName} from party", ToastType.Warning, 2f);
            
            LoadParty();
        }
        
        private void OnCloseClicked()
        {
            Hide(true);
        }
        
        protected override void OnDispose()
        {
            if (_closeButton != null)
            {
                _closeButton.clicked -= OnCloseClicked;
            }
        }
        
        // Mock data - replace with actual EOS/Network data
        private List<FriendInfo> GetMockFriends()
        {
            return new List<FriendInfo>
            {
                new FriendInfo { displayName = "ChefMaster99", isOnline = true },
                new FriendInfo { displayName = "CookingKing", isOnline = true },
                new FriendInfo { displayName = "FoodieQueen", isOnline = false },
                new FriendInfo { displayName = "GrillMaster", isOnline = true },
                new FriendInfo { displayName = "SushiSensei", isOnline = false }
            };
        }
        
        private List<PartyMember> GetMockPartyMembers()
        {
            return new List<PartyMember>
            {
                new PartyMember 
                { 
                    displayName = "You", 
                    isLeader = true, 
                    isReady = true, 
                    isLocal = true,
                    canKick = false 
                }
            };
        }
    }
    
    // Data structures - move to separate file if needed
    public class FriendInfo
    {
        public string displayName;
        public bool isOnline;
    }
    
    public class PartyMember
    {
        public string displayName;
        public bool isLeader;
        public bool isReady;
        public bool isLocal;
        public bool canKick;
    }
}
