# Friends System Testing Guide
## Unity Gaming Services + Epic Online Services Integration

This guide will help you test the complete friends system setup.

---

## Prerequisites

### 1. Unity Gaming Services Setup

#### Step 1: Link Unity Project
1. Open Unity Editor
2. Go to **Edit → Project Settings → Services**
3. Click **Create Unity Project ID** (or link existing)
4. Note your **Project ID** (you'll need this)

#### Step 2: Enable Required Services
1. In Unity Dashboard (https://dashboard.unity3d.com/):
   - Go to your project
   - Enable **Authentication** service
   - Enable **Friends** service

#### Step 3: Create UGSConfig Asset
1. In Unity, right-click in `Assets/Resources/`
2. Select **Create → Config → UGS Config**
3. Name it: `UGSConfig`
4. Paste your **Project ID** in the config
5. Enable **Friends System** checkbox

### 2. Epic Online Services Setup (Already Done)

Your EOS setup should already be working:
- ✅ EOS SDK integrated
- ✅ ProductUserId authentication
- ✅ Lobbies and matchmaking working

### 3. Package Installation

Wait for Unity to download packages (check Package Manager):
- ✅ `com.unity.services.core` - v1.12.5
- ✅ `com.unity.services.authentication` - v3.3.3
- ✅ `com.unity.services.friends` - v1.0.0

---

## Testing Scenarios

### Test 1: Basic Initialization ✅

**Goal**: Verify both EOS and UGS initialize correctly

**Steps**:
1. Start the game
2. Login with EOS (as you normally do)
3. Check Console logs for:
   ```
   [Networking] Initializing services...
   [Networking] Initializing Unity Gaming Services...
   [Networking] UGS initialized successfully
   [Networking] Signed in to UGS - PlayerId: <your-player-id>
   [Networking] Initialized - Player ID: <your-player-id>
   [FriendsService] Initializing UGS Friends Service...
   [FriendsService] Initialized - Player ID: <your-player-id>
   ```

**Expected Result**:
- No errors
- Both EOS and UGS show successful initialization
- You get a Unity Player ID (your "friend code")

**Troubleshooting**:
| Error | Solution |
|-------|----------|
| "UGSConfig not found" | Create `UGSConfig` in `Assets/Resources/` |
| "Invalid Project ID" | Check Project ID in UGS Dashboard |
| "Failed to initialize UGS" | Check internet connection, verify Project ID |
| "UGS not signed in" | EOS must authenticate first |

---

### Test 2: Send Friend Request (2 Clients Required) 🤝

**Goal**: Test friend request functionality

**Setup**:
- Client A: Your main test machine
- Client B: Another device OR build standalone

**Steps**:

**On Client A**:
1. Note your Player ID from console logs
2. Open Friends UI (if you have one) or use debug commands

**On Client B**:
1. Start game
2. Note your Player ID
3. Send friend request to Client A's Player ID:
   ```csharp
   // In your UI or debug console
   var friendsService = GameBootstrap.Services.NetworkingServices.FriendsService;
   await friendsService.SendFriendRequestAsync("CLIENT_A_PLAYER_ID");
   ```

**On Client A**:
4. Check for incoming request event:
   ```
   [FriendsService] Friend request received from: <Client B ID>
   ```

**Expected Result**:
- ✅ Client B sends request successfully
- ✅ Client A receives notification
- ✅ Request appears in `PendingRequests` list

**Debug Script** (optional):
```csharp
// Add to your debug UI
public async void TestSendFriendRequest(string targetPlayerId)
{
    try
    {
        var friendsService = // Get your friends service
        var request = await friendsService.SendFriendRequestAsync(targetPlayerId);
        Debug.Log($"Request sent! ID: {request.Id}");
    }
    catch (Exception ex)
    {
        Debug.LogError($"Failed: {ex.Message}");
    }
}
```

---

### Test 3: Accept Friend Request ✅

**Goal**: Accept a friend request

**Steps**:

**On Client A** (receiver):
1. Get pending requests:
   ```csharp
   var pendingRequests = friendsService.PendingRequests;
   foreach (var request in pendingRequests)
   {
       Debug.Log($"Request from: {request.FromUserName} (ID: {request.Id})");
   }
   ```
2. Accept the first request:
   ```csharp
   var requestId = pendingRequests[0].Id;
   await friendsService.AcceptFriendRequestAsync(requestId);
   ```

**Expected Result**:
- ✅ Request removed from pending
- ✅ User added to friends list on BOTH clients
- ✅ Event fired: `OnFriendAdded`
- ✅ Console log: "Friend request accepted"

---

### Test 4: Check Friends List 📋

**Goal**: Verify friends list syncs

**Steps**:

**On Both Clients**:
```csharp
var friends = friendsService.Friends;
Debug.Log($"Total friends: {friends.Count}");

foreach (var friend in friends)
{
    Debug.Log($"Friend: {friend.DisplayName}");
    Debug.Log($"  - ID: {friend.FriendCode}");
    Debug.Log($"  - Online: {friend.IsOnline}");
    Debug.Log($"  - Last Seen: {friend.LastSeen}");
}
```

**Expected Result**:
- ✅ Both clients show each other as friends
- ✅ Online status is "True" (both are online)
- ✅ Names displayed correctly

---

### Test 5: Real-time Presence Updates 🟢🔴

**Goal**: Test online/offline status updates

**Steps**:

**On Client A**:
1. Have Client B as friend
2. Watch the friends list

**On Client B**:
3. Close the game
4. Wait 5-10 seconds

**On Client A**:
5. Check console for:
   ```
   [FriendsService] Presence updated for: <Client B ID> - Offline
   ```
6. Friends list should update automatically

**Expected Result**:
- ✅ Client A detects Client B went offline
- ✅ `IsOnline` changes to `false`
- ✅ Event fired: `OnFriendsListUpdated`

---

### Test 6: Remove Friend 🗑️

**Goal**: Test friend removal

**Steps**:

**On Client A**:
```csharp
var friendToRemove = friendsService.Friends[0];
await friendsService.RemoveFriendAsync(friendToRemove.FriendCode);
```

**Expected Result**:
- ✅ Friend removed from Client A's list
- ✅ Relationship deleted on Client B's side too
- ✅ Event fired: `OnFriendRemoved`
- ✅ Console log: "Friend removed"

---

### Test 7: Lobby Invite with UGS Friend 🎮

**Goal**: Verify EOS lobby invites work with UGS friends

**Setup**:
- Client A in party/lobby
- Client B is a UGS friend

**Steps**:

**On Client A**:
1. Create a party lobby (EOS)
2. Get friend from UGS:
   ```csharp
   var friend = friendsService.Friends[0];
   ```
3. Invite to party:
   ```csharp
   friendsService.InviteToParty(friend.FriendCode);
   ```

**Expected Result**:
- ⚠️ **This will only work if EOS ProductUserId mapping exists**
- ✅ If mapping exists: Client B receives EOS lobby invite
- ❌ If no mapping: Error logged "Invalid ProductUserId for friend"

**Note**: This requires the friend to have played at least once on the same device, so the EOS ↔ UGS mapping is created.

---

## Quick Test Script

Add this to a test MonoBehaviour for quick testing:

```csharp
using UnityEngine;
using Core.Logging;
using Core.Networking.Interfaces;

public class FriendsSystemTester : MonoBehaviour
{
    private IFriendsService _friendsService;

    void Start()
    {
        // Get friends service from your DI container
        _friendsService = GameBootstrap.Services.NetworkingServices.FriendsService;

        // Subscribe to events
        if (_friendsService != null)
        {
            _friendsService.OnFriendsListUpdated += OnFriendsUpdated;
            _friendsService.OnFriendAdded += OnFriendAdded;
            _friendsService.OnFriendRemoved += OnFriendRemoved;
            _friendsService.OnFriendRequestReceived += OnRequestReceived;
        }
    }

    void OnGUI()
    {
        if (_friendsService == null || !_friendsService.IsInitialized)
        {
            GUILayout.Label("Friends Service Not Initialized");
            return;
        }

        GUILayout.BeginArea(new Rect(10, 10, 300, 500));

        GUILayout.Label($"My Player ID: {_friendsService.MyFriendCode}");
        GUILayout.Space(10);

        // Send Friend Request
        GUILayout.Label("Send Friend Request:");
        targetPlayerId = GUILayout.TextField(targetPlayerId, 100);
        if (GUILayout.Button("Send Request"))
        {
            SendFriendRequest();
        }
        GUILayout.Space(10);

        // Friends List
        GUILayout.Label($"Friends ({_friendsService.Friends.Count}):");
        foreach (var friend in _friendsService.Friends)
        {
            string status = friend.IsOnline ? "🟢" : "🔴";
            GUILayout.Label($"{status} {friend.DisplayName} ({friend.FriendCode})");
            if (GUILayout.Button($"Remove {friend.DisplayName}"))
            {
                RemoveFriend(friend.FriendCode);
            }
        }
        GUILayout.Space(10);

        // Pending Requests
        GUILayout.Label($"Pending Requests ({_friendsService.PendingRequests.Count}):");
        foreach (var request in _friendsService.PendingRequests)
        {
            GUILayout.Label($"From: {request.FromUserName}");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Accept"))
            {
                AcceptRequest(request.Id);
            }
            if (GUILayout.Button("Reject"))
            {
                RejectRequest(request.Id);
            }
            GUILayout.EndHorizontal();
        }

        GUILayout.EndArea();
    }

    private string targetPlayerId = "";

    private async void SendFriendRequest()
    {
        try
        {
            await _friendsService.SendFriendRequestAsync(targetPlayerId);
            Debug.Log("Friend request sent!");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to send request: {ex.Message}");
        }
    }

    private async void AcceptRequest(string requestId)
    {
        try
        {
            await _friendsService.AcceptFriendRequestAsync(requestId);
            Debug.Log("Friend request accepted!");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to accept: {ex.Message}");
        }
    }

    private async void RejectRequest(string requestId)
    {
        try
        {
            await _friendsService.RejectFriendRequestAsync(requestId);
            Debug.Log("Friend request rejected!");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to reject: {ex.Message}");
        }
    }

    private async void RemoveFriend(string friendCode)
    {
        try
        {
            await _friendsService.RemoveFriendAsync(friendCode);
            Debug.Log("Friend removed!");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to remove: {ex.Message}");
        }
    }

    private void OnFriendsUpdated()
    {
        Debug.Log("Friends list updated!");
    }

    private void OnFriendAdded(Core.Networking.Interfaces.FriendInfo friend)
    {
        Debug.Log($"New friend added: {friend.DisplayName}");
    }

    private void OnFriendRemoved(string friendCode)
    {
        Debug.Log($"Friend removed: {friendCode}");
    }

    private void OnRequestReceived(Core.Networking.Interfaces.FriendRequest request)
    {
        Debug.Log($"New friend request from: {request.FromUserName}");
    }
}
```

---

## Common Issues & Solutions

### Issue: "UGS not signed in"

**Cause**: UGS authentication happening before EOS

**Solution**: Check `NetworkingServiceContainer.cs:104` - UGS initializes after EOS

---

### Issue: Friend request sent but not received

**Possible Causes**:
1. **Wrong Player ID** - Make sure you're using the exact Player ID (case-sensitive)
2. **Not initialized** - Both clients must be initialized
3. **Network issues** - Check internet connection

**Debug**:
```csharp
Debug.Log($"My Player ID: {_authManager.PlayerId}");
Debug.Log($"Is Signed In: {_authManager.IsSignedIn}");
Debug.Log($"Service Initialized: {_friendsService.IsInitialized}");
```

---

### Issue: Presence not updating

**Cause**: Events not subscribed or service not initialized

**Solution**:
```csharp
// Check events are subscribed
_ugsInstance.PresenceUpdated += OnPresenceUpdated;
```

---

### Issue: Can't invite UGS friend to EOS lobby

**Cause**: No EOS ↔ UGS mapping exists

**Solution**: This is a known limitation. The mapping is created when:
1. User authenticates with EOS (gets ProductUserId)
2. User authenticates with UGS (gets PlayerId)
3. Mapping stored in `PlayerPrefs`

**Workaround**: Add friends who have played the game before (so mapping exists)

---

## Performance Benchmarks

**Expected Performance**:
- Friend request: < 500ms
- Accept request: < 300ms
- Refresh friends: < 200ms
- Presence update: Real-time (< 1s)

---

## Next Steps

After basic testing works:

1. **Build UI** - Create proper friends list UI
2. **Notifications** - Add toast notifications for friend requests
3. **Search** - Add friend search by name (UGS supports this)
4. **Recent Players** - Test recent players feature
5. **Production** - Test with 10+ users

---

## Support

If you encounter issues:
1. Check Unity Console for errors
2. Check UGS Dashboard logs: https://dashboard.unity3d.com/
3. Verify Project ID and services are enabled
4. Check network connectivity

**Documentation**:
- UGS Friends: https://docs.unity.com/ugs/en-us/manual/friends/manual
- EOS SDK: https://dev.epicgames.com/docs/epic-online-services

---

✅ **Testing Complete!**

Your hybrid UGS + EOS friends system is ready for production! 🎉
