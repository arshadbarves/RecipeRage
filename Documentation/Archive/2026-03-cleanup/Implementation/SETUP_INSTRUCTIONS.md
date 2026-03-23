# Network Setup Instructions

## 🚀 Automated Setup (Recommended)

### Option 1: Use the Network Setup Wizard

1. **Open Unity Editor**
2. **Go to Menu**: `RecipeRage > Network Setup Wizard`
3. **Click**: "Complete Setup (All Steps)" button
4. **Done!** The wizard will automatically:
   - Create NetworkManagers GameObject
   - Setup NetworkManager
   - Add StationNetworkController to all stations
   - Add NetworkObject to all prefabs
   - Register prefabs in NetworkManager

### Option 2: Step-by-Step with Wizard

1. Open `RecipeRage > Network Setup Wizard`
2. Click each step button in order:
   - Step 1: Create Network Managers GameObject
   - Step 2: Setup NetworkManager
   - Step 3: Add StationNetworkController to Stations
   - Step 4: Add NetworkObject to Prefabs
   - Step 5: Register Prefabs in NetworkManager

---

## 🔧 Manual Setup (If Needed)

### Step 1: Create NetworkManagers GameObject

In your **Game** scene:

1. Create new GameObject: `GameObject > Create Empty`
2. Name it: "NetworkManagers"
3. Add these components:
   - `NetworkGameStateManager`
   - `NetworkScoreManager`
   - `RoundTimer`
   - `IngredientNetworkSpawner`
   - `NetworkInitializer` (important!)

### Step 2: Setup NetworkManager

1. Find or create `NetworkManager` GameObject
2. Configure settings:
   - Tick Rate: 30
   - Transport: EOSTransport (should already be configured)

### Step 3: Configure Station Prefabs

For each station prefab in `Assets/Prefabs/Stations/`:
- CookingPot.prefab
- CuttingStation.prefab
- AssemblyStation.prefab
- ServingStation.prefab

Add these components:
1. `NetworkObject` (if not present)
2. `StationNetworkController`

### Step 4: Configure Other Prefabs

**Player Prefab** (`Assets/Prefabs/Player/Player.prefab`):
- Ensure `NetworkObject` component exists
- Ensure `PlayerController` component exists

**Plate Prefab** (`Assets/Prefabs/Plate.prefab`):
- Add `NetworkObject` component
- Add `PlateItem` component
- Configure ingredient slots (optional)

**Ingredient Prefab** (if you have one):
- Add `NetworkObject` component
- Add `IngredientItem` component

### Step 5: Register Prefabs in NetworkManager

1. Select `NetworkManager` GameObject
2. In Inspector, find `NetworkConfig > Prefabs`
3. Add these prefabs to the list:
   - Player prefab
   - All station prefabs
   - Plate prefab
   - Ingredient prefab (if you have one)
4. Set `Player Prefab` field to your Player prefab

---

## ✅ Verification Checklist

After setup, verify:

### Scene Setup
- [ ] NetworkManagers GameObject exists in Game scene
- [ ] NetworkGameStateManager component added
- [ ] NetworkScoreManager component added
- [ ] RoundTimer component added
- [ ] IngredientNetworkSpawner component added
- [ ] NetworkInitializer component added
- [ ] NetworkManager GameObject exists

### Prefab Configuration
- [ ] Player prefab has NetworkObject
- [ ] All station prefabs have NetworkObject
- [ ] All station prefabs have StationNetworkController
- [ ] Plate prefab has NetworkObject and PlateItem
- [ ] All prefabs registered in NetworkManager
- [ ] Player prefab set as PlayerPrefab in NetworkManager

### Transport Configuration
- [ ] EOSTransport component exists
- [ ] EOSTransport configured with your EOS credentials

---

## 🧪 Testing the Setup

### Test 1: Scene Validation
1. Open Game scene
2. Check Console for errors
3. Verify all components are present

### Test 2: Play Mode (Host)
1. Enter Play Mode
2. Start as Host (via your lobby UI)
3. Check Console for:
   - "[NetworkInitializer] Network services initialized"
   - "[NetworkGameManager] ..."
   - No errors

### Test 3: Player Spawn
1. Start as Host
2. Verify player spawns
3. Check Console for:
   - "[NetworkGameManager] Spawned player..."
   - "[PlayerNetworkManager] Registered player..."

### Test 4: Network Components
1. In Play Mode, check Hierarchy
2. Verify NetworkManagers components are active
3. Check NetworkManager shows connected clients

---

## 🔍 Troubleshooting

### Issue: "NetworkManager.Singleton is null"
**Solution**: Ensure NetworkManager GameObject exists in the scene

### Issue: "Player doesn't spawn"
**Solution**: 
- Check Player prefab is set in NetworkManager
- Check Player prefab has NetworkObject component
- Check Player prefab is in NetworkPrefabs list

### Issue: "Services are null"
**Solution**: 
- Ensure GameBootstrap is initialized first
- Check ServiceContainer has network services registered
- Verify NetworkInitializer is in the scene

### Issue: "Stations don't have NetworkObject"
**Solution**: Run the Network Setup Wizard Step 4

### Issue: "Prefabs not registered"
**Solution**: Run the Network Setup Wizard Step 5

---

## 📝 Next Steps After Setup

Once setup is complete:

1. **Test Locally**: 
   - Start as host
   - Verify player spawns
   - Test ingredient interactions

2. **Update Station Scripts**:
   - Follow `QUICK_START_GUIDE.md` Example 5
   - Add StationNetworkController integration

3. **Connect UI**:
   - Follow `QUICK_START_GUIDE.md` Example 4
   - Subscribe to network events

4. **Test Multiplayer**:
   - Build the game
   - Test host + client

---

## 🎯 Quick Reference

### Required Components in Scene
```
Game Scene
├── NetworkManager (Unity Netcode)
├── NetworkManagers
│   ├── NetworkGameStateManager
│   ├── NetworkScoreManager
│   ├── RoundTimer
│   ├── IngredientNetworkSpawner
│   └── NetworkInitializer
└── GameBootstrap (existing)
```

### Required Components on Prefabs
```
Player.prefab
├── NetworkObject
└── PlayerController

Station.prefab
├── NetworkObject
├── CookingStation (or subclass)
└── StationNetworkController

Plate.prefab
├── NetworkObject
└── PlateItem
```

---

## 🆘 Need Help?

- Check `QUICK_START_GUIDE.md` for code examples
- Check `NETCODE_IMPLEMENTATION_PLAN.md` for architecture
- Check `IMPLEMENTATION_CHECKLIST.md` for progress tracking
- Check Console for error messages
- Verify all prefabs have required components

---

## ✨ Success!

Once setup is complete, you should see:
- ✅ No errors in Console
- ✅ NetworkManager shows in Hierarchy
- ✅ NetworkManagers GameObject with all components
- ✅ All prefabs configured
- ✅ Ready for multiplayer testing!

Proceed to `QUICK_START_GUIDE.md` for code integration examples.
