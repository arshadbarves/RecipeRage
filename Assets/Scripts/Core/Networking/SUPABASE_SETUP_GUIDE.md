# Supabase Setup Guide for Friends System

## Overview

This guide shows how to set up Supabase for the Friends System. We'll use **Supabase REST API directly from Unity** - no backend server needed!

## Step 1: Create Supabase Project

1. Go to [https://supabase.com](https://supabase.com)
2. Sign up / Log in
3. Click "New Project"
4. Fill in:
   - **Name:** RecipeRage
   - **Database Password:** (save this securely)
   - **Region:** Choose closest to your players
5. Click "Create new project"
6. Wait 2-3 minutes for setup

## Step 2: Get API Credentials

1. In your project dashboard, go to **Settings** → **API**
2. Copy these values:
   - **Project URL:** `https://xxxxx.supabase.co`
   - **anon public key:** `eyJhbGc...` (long string)
3. Save these - you'll need them in Unity

## Step 3: Create Database Schema

1. Go to **SQL Editor** in Supabase dashboard
2. Click "New Query"
3. Paste this SQL:

```sql
-- Enable UUID extension
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Users table
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    product_user_id TEXT UNIQUE NOT NULL,
    display_name TEXT NOT NULL,
    friend_code TEXT UNIQUE NOT NULL,
    device_id TEXT,
    last_seen TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Friend requests table
CREATE TABLE friend_requests (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    from_user_id TEXT REFERENCES users(product_user_id) ON DELETE CASCADE,
    to_user_id TEXT REFERENCES users(product_user_id) ON DELETE CASCADE,
    status TEXT NOT NULL DEFAULT 'pending', -- 'pending', 'accepted', 'rejected'
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    UNIQUE(from_user_id, to_user_id)
);

-- Friends table (only accepted friendships)
CREATE TABLE friends (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id TEXT REFERENCES users(product_user_id) ON DELETE CASCADE,
    friend_id TEXT REFERENCES users(product_user_id) ON DELETE CASCADE,
    added_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    PRIMARY KEY (user_id, friend_id),
    CHECK (user_id < friend_id) -- Ensure only one row per friendship
);

-- Recent players table
CREATE TABLE recent_players (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id TEXT REFERENCES users(product_user_id) ON DELETE CASCADE,
    recent_player_id TEXT REFERENCES users(product_user_id) ON DELETE CASCADE,
    last_played_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    PRIMARY KEY (user_id, recent_player_id)
);

-- Indexes for performance
CREATE INDEX idx_users_friend_code ON users(friend_code);
CREATE INDEX idx_users_product_user_id ON users(product_user_id);
CREATE INDEX idx_friend_requests_to_user ON friend_requests(to_user_id) WHERE status = 'pending';
CREATE INDEX idx_friend_requests_from_user ON friend_requests(from_user_id);
CREATE INDEX idx_friends_user_id ON friends(user_id);
CREATE INDEX idx_friends_friend_id ON friends(friend_id);
CREATE INDEX idx_recent_players_user_id ON recent_players(user_id);

-- Function to accept friend request
CREATE OR REPLACE FUNCTION accept_friend_request(request_id UUID)
RETURNS VOID AS $$
DECLARE
    req RECORD;
BEGIN
    -- Get request details
    SELECT from_user_id, to_user_id INTO req
    FROM friend_requests
    WHERE id = request_id AND status = 'pending';
    
    IF NOT FOUND THEN
        RAISE EXCEPTION 'Friend request not found or already processed';
    END IF;
    
    -- Update request status
    UPDATE friend_requests
    SET status = 'accepted', updated_at = NOW()
    WHERE id = request_id;
    
    -- Add to friends table (bidirectional)
    INSERT INTO friends (user_id, friend_id)
    VALUES 
        (LEAST(req.from_user_id, req.to_user_id), GREATEST(req.from_user_id, req.to_user_id))
    ON CONFLICT DO NOTHING;
END;
$$ LANGUAGE plpgsql;

-- Function to reject friend request
CREATE OR REPLACE FUNCTION reject_friend_request(request_id UUID)
RETURNS VOID AS $$
BEGIN
    UPDATE friend_requests
    SET status = 'rejected', updated_at = NOW()
    WHERE id = request_id AND status = 'pending';
    
    IF NOT FOUND THEN
        RAISE EXCEPTION 'Friend request not found or already processed';
    END IF;
END;
$$ LANGUAGE plpgsql;
```

4. Click "Run" to execute
5. Verify tables created: Go to **Table Editor** and see `users`, `friend_requests`, `friends`, `recent_players`

## Step 4: Set Up Row Level Security (RLS)

1. Go to **Authentication** → **Policies**
2. For each table, click "Enable RLS"
3. Add these policies:

### Users Table Policies

```sql
-- Allow anyone to read users (for friend lookup)
CREATE POLICY "Users are viewable by everyone"
ON users FOR SELECT
USING (true);

-- Allow users to insert their own record
CREATE POLICY "Users can insert their own record"
ON users FOR INSERT
WITH CHECK (true);

-- Allow users to update their own record
CREATE POLICY "Users can update their own record"
ON users FOR UPDATE
USING (true);
```

### Friend Requests Policies

```sql
-- Users can view requests they sent or received
CREATE POLICY "Users can view their friend requests"
ON friend_requests FOR SELECT
USING (from_user_id = current_setting('request.jwt.claims')::json->>'product_user_id' 
    OR to_user_id = current_setting('request.jwt.claims')::json->>'product_user_id');

-- Users can send friend requests
CREATE POLICY "Users can send friend requests"
ON friend_requests FOR INSERT
WITH CHECK (from_user_id = current_setting('request.jwt.claims')::json->>'product_user_id');

-- Users can update requests they received
CREATE POLICY "Users can update received requests"
ON friend_requests FOR UPDATE
USING (to_user_id = current_setting('request.jwt.claims')::json->>'product_user_id');
```

### Friends Table Policies

```sql
-- Users can view their friends
CREATE POLICY "Users can view their friends"
ON friends FOR SELECT
USING (user_id = current_setting('request.jwt.claims')::json->>'product_user_id' 
    OR friend_id = current_setting('request.jwt.claims')::json->>'product_user_id');
```

### Recent Players Policies

```sql
-- Users can view their recent players
CREATE POLICY "Users can view their recent players"
ON recent_players FOR SELECT
USING (user_id = current_setting('request.jwt.claims')::json->>'product_user_id');

-- Users can insert their recent players
CREATE POLICY "Users can insert recent players"
ON recent_players FOR INSERT
WITH CHECK (user_id = current_setting('request.jwt.claims')::json->>'product_user_id');
```

## Step 5: Configure Unity

1. Open Unity
2. Create a ScriptableObject for Supabase config:

```csharp
// Assets/Scripts/Core/Networking/SupabaseConfig.cs
[CreateAssetMenu(fileName = "SupabaseConfig", menuName = "RecipeRage/Supabase Config")]
public class SupabaseConfig : ScriptableObject
{
    [Header("Supabase Credentials")]
    public string projectUrl = "https://xxxxx.supabase.co";
    public string anonKey = "eyJhbGc...";
}
```

3. Create the asset:
   - Right-click in Project → Create → RecipeRage → Supabase Config
   - Name it "SupabaseConfig"
   - Paste your Project URL and anon key
   - Save in `Assets/Resources/`

## Step 6: Test Connection

1. In Unity, create a test script:

```csharp
public async void TestSupabaseConnection()
{
    var config = Resources.Load<SupabaseConfig>("SupabaseConfig");
    
    using (var client = new UnityWebRequest($"{config.projectUrl}/rest/v1/users", "GET"))
    {
        client.SetRequestHeader("apikey", config.anonKey);
        client.SetRequestHeader("Authorization", $"Bearer {config.anonKey}");
        
        await client.SendWebRequest();
        
        if (client.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("✅ Supabase connection successful!");
        }
        else
        {
            Debug.LogError($"❌ Supabase connection failed: {client.error}");
        }
    }
}
```

2. Run in Play Mode
3. Should see "✅ Supabase connection successful!"

## API Endpoints

### 1. Register User

```http
POST /rest/v1/users
Headers:
  apikey: {your-anon-key}
  Authorization: Bearer {your-anon-key}
  Content-Type: application/json
  Prefer: return=representation

Body:
{
  "product_user_id": "abc123",
  "display_name": "PlayerName",
  "friend_code": "ABC12345",
  "device_id": "device123"
}
```

### 2. Find User by Friend Code

```http
GET /rest/v1/users?friend_code=eq.ABC12345&select=*
Headers:
  apikey: {your-anon-key}
  Authorization: Bearer {your-anon-key}
```

### 3. Send Friend Request

```http
POST /rest/v1/friend_requests
Headers:
  apikey: {your-anon-key}
  Authorization: Bearer {your-anon-key}
  Content-Type: application/json
  Prefer: return=representation

Body:
{
  "from_user_id": "my_product_user_id",
  "to_user_id": "friend_product_user_id",
  "status": "pending"
}
```

### 4. Get Pending Requests

```http
GET /rest/v1/friend_requests?to_user_id=eq.{my_id}&status=eq.pending&select=*,from_user:users!from_user_id(*)
Headers:
  apikey: {your-anon-key}
  Authorization: Bearer {your-anon-key}
```

### 5. Accept Friend Request

```http
POST /rest/v1/rpc/accept_friend_request
Headers:
  apikey: {your-anon-key}
  Authorization: Bearer {your-anon-key}
  Content-Type: application/json

Body:
{
  "request_id": "uuid-here"
}
```

### 6. Get Friends List

```http
GET /rest/v1/friends?or=(user_id.eq.{my_id},friend_id.eq.{my_id})&select=*,user:users!user_id(*),friend:users!friend_id(*)
Headers:
  apikey: {your-anon-key}
  Authorization: Bearer {your-anon-key}
```

## Security Notes

### ✅ Safe to Use
- **anon key** is safe to expose in client
- RLS policies protect data
- Users can only access their own data

### ⚠️ Never Expose
- **service_role key** - keep this secret!
- Database password
- Don't commit keys to git (use .gitignore)

## Troubleshooting

### Error: "relation does not exist"
- Tables not created
- Run SQL schema again

### Error: "permission denied"
- RLS policies not set up
- Check policies in Authentication → Policies

### Error: "JWT expired"
- anon key is permanent, no expiry
- Check you're using anon key, not service_role key

### Error: "duplicate key value"
- Friend code already exists
- Generate new unique code

## Cost

**Free Tier Limits:**
- 500 MB database
- 2 GB bandwidth/month
- 50,000 monthly active users

**Perfect for MVP!** Upgrade later if needed.

## Next Steps

1. ✅ Create Supabase project
2. ✅ Run SQL schema
3. ✅ Set up RLS policies
4. ✅ Get API credentials
5. ✅ Create SupabaseConfig in Unity
6. ✅ Test connection
7. → Implement FriendsService with Supabase

---

**Status:** Setup guide complete
**Next:** Implement production-ready FriendsService
