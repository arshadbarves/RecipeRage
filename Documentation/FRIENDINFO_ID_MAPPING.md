# FriendInfo ID Mapping Explanation

## The Problem

`FriendsService` had two methods that tried to do reverse lookups:
1. `GetPlayerIdFromEosId()` - EOS ProductUserId → Unity PlayerId
2. `GetEosIdFromPlayerId()` - Unity PlayerId → EOS ProductUserId

These methods were removed because **reverse lookups aren't possible without server-side tracking**.

---

## Why Reverse Lookups Don't Work

### Unity's External Identity System:
```
Unity Server stores:
EOS "0002d4f6..." → Unity "player-xyz-123" ✅ (forward lookup works)

But doesn't provide reverse lookup:
Unity "player-xyz-123" → EOS "0002d4f6..." ❌ (not available via API)
```

Unity only provides the **forward lookup** when you authenticate. There's no API to query backwards.

---

## The Solution

### FriendInfo Structure:
```csharp
public class FriendInfo
{
    public string FriendCode { get; set; }        // Can be Unity PlayerId OR EOS ProductUserId
    public ProductUserId ProductUserId { get; set; } // EOS ProductUserId (nullable)
    public string DisplayName { get; set; }
    // ... other fields
}
```

### Two Types of Friends:

#### 1. **Friends from Unity Friends API** (added via Unity Friends system)
```csharp
_friends.Add(new FriendInfo
{
    FriendCode = member.Id,        // Unity PlayerId (e.g., "player-xyz-123")
    ProductUserId = null,          // ❌ Can't reverse-lookup EOS ID
    DisplayName = profile?.Name ?? "Unknown",
    IsOnline = isOnline,
    IsRecent = false
});
```

**Characteristics**:
- ✅ Added through Unity Friends API
- ✅ Have Unity PlayerId
- ❌ Don't have EOS ProductUserId (can't reverse-lookup)
- ✅ Can still send lobby invites through Unity system

#### 2. **Recent Players from Lobbies** (met in EOS lobbies)
```csharp
_recentPlayers.Insert(0, new FriendInfo
{
    FriendCode = playerIdStr,      // EOS ProductUserId (e.g., "0002d4f6...")
    ProductUserId = productUserId, // ✅ Have EOS ProductUserId
    DisplayName = displayName,
    IsOnline = true,
    IsRecent = true
});
```

**Characteristics**:
- ✅ Met in EOS lobbies (have their EOS ProductUserId)
- ✅ Have EOS ProductUserId
- ❌ Don't have Unity PlayerId (can't reverse-lookup)
- ✅ Can invite directly to lobbies using EOS ProductUserId

---

## How It Works in Practice

### Scenario 1: Adding a Friend via Unity Friends
```
1. User searches for friend by Unity PlayerId
2. Sends friend request through Unity Friends API
3. Friend accepts
4. Friend appears in your list:
   - FriendCode: "player-xyz-123" (Unity ID)
   - ProductUserId: null
   - DisplayName: "John"
5. To invite to lobby: Send Unity-based invite (Unity handles it)
```

### Scenario 2: Playing with Someone in a Lobby
```
1. Join lobby, meet player "Jane"
2. Get their EOS ProductUserId from lobby member list
3. Add to recent players:
   - FriendCode: "0002d4f6..." (EOS ID)
   - ProductUserId: 0002d4f6...
   - DisplayName: "Jane"
4. To invite to lobby again: Use EOS ProductUserId directly
```

### Scenario 3: Recent Player Becomes Friend
```
1. Player "Jane" is in recent players (have EOS ID)
2. You add her via Unity Friends (now have Unity ID too)
3. She appears in BOTH lists:
   - Recent: FriendCode = EOS ID
   - Friends: FriendCode = Unity ID
4. Both work for invites (use whichever is available)
```

---

## Code Changes Made

### File: `FriendsService.cs`

**Line 297 - AddRecentPlayer() - FIXED:**
```csharp
// OLD (ERROR):
var unityPlayerId = _authManager.GetPlayerIdFromEosId(playerIdStr);
FriendCode = unityPlayerId ?? playerIdStr,

// NEW (FIXED):
FriendCode = playerIdStr, // Use EOS ProductUserId directly
```

**Reason**: Recent players come from lobbies, we only have their EOS ID, so use that.

**Line 379 - UpdateLocalFriends() - FIXED:**
```csharp
// OLD (ERROR):
var eosProductUserId = _authManager.GetEosIdFromPlayerId(member.Id);
ProductUserId = ProductUserId.FromString(eosProductUserId);

// NEW (FIXED):
ProductUserId = null, // Can't reverse-lookup
```

**Reason**: Friends from Unity API, we only have their Unity PlayerId, can't get EOS ID.

---

## Impact on Lobby Invites

### Inviting Friends to Lobbies:

#### Option 1: Friend has EOS ProductUserId (recent players)
```csharp
if (friend.ProductUserId != null)
{
    // Direct EOS lobby invite
    LobbyManager.InviteToLobby(friend.ProductUserId);
}
```

#### Option 2: Friend has Unity PlayerId only (Unity friends)
```csharp
if (friend.ProductUserId == null)
{
    // Send invite through Unity system
    // Unity will handle routing to their device
    UnityFriendsService.SendInvite(friend.FriendCode, lobbyId);
}
```

**Both work!** It's just two different paths to the same outcome.

---

## Future Enhancement (Optional)

If you need full bidirectional mapping, you'd need server-side tracking:

### Option A: Add Backend API
```csharp
// Backend stores both mappings:
public class UserIdentity
{
    public string UnityPlayerId { get; set; }
    public string EosProductUserId { get; set; }
}

// API endpoints:
GET /api/users/by-unity/{unityId}  → returns EOS ID
GET /api/users/by-eos/{eosId}     → returns Unity ID
```

### Option B: Use Unity Cloud Save
```csharp
// Store mapping in Unity Cloud Save
await CloudSave.SaveDataAsync(new Dictionary<string, object>
{
    { "EosProductUserId", _authManager.EosProductUserId }
});

// Other players can query this
var data = await CloudSave.LoadDataAsync(unityPlayerId);
var eosId = data["EosProductUserId"];
```

**Current Implementation**: Neither option is implemented (not needed for basic functionality).

---

## Summary

### ✅ What Works:
- Friends from Unity Friends API (have Unity PlayerId)
- Recent players from lobbies (have EOS ProductUserId)
- Inviting either type to lobbies (different methods)

### ❌ What Doesn't Work:
- Reverse lookups (Unity ID → EOS ID or vice versa)
- Requires server-side tracking to implement

### 💡 Best Practice:
Use whichever ID you have available:
- **Unity Friends**: Use Unity PlayerId
- **Recent Players**: Use EOS ProductUserId
- **Both work for their respective purposes!**

---

## Testing

### Test 1: Add Friend via Unity
```
1. Search for friend by Unity PlayerId
2. Send friend request
3. Check friend list
   ✅ FriendCode = Unity PlayerId
   ✅ ProductUserId = null (expected)
   ✅ Can send invites via Unity system
```

### Test 2: Play with Someone in Lobby
```
1. Join lobby with random player
2. Check recent players
   ✅ FriendCode = EOS ProductUserId
   ✅ ProductUserId = EOS ProductUserId (expected)
   ✅ Can invite directly to lobby
```

### Test 3: Both Types Coexist
```
1. Have Unity friends (FriendCode = Unity ID)
2. Have recent players (FriendCode = EOS ID)
3. Both lists display correctly
   ✅ No conflicts
   ✅ Both types functional
```

The system works correctly with these two distinct friend types!
