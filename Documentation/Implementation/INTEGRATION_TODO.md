# Integration TODO - What Actually Needs to Be Done

## ⚠️ Reality Check

The netcode **infrastructure is complete**, but your **existing gameplay code** needs updates to use it.

---

## 🎯 **Critical Path (Must Do for Multiplayer to Work)**

### 1. Update CookingStation Base Class (30 min)
**File**: `Assets/Scripts/Gameplay/Stations/CookingStation.cs`

**Current Issue**: Has NetworkBehaviour but doesn't use StationNetworkController

**What to Add**:
```csharp
// Add at top of class
private StationNetworkController _networkController;

// In Awake()
protected override void Awake()
{
    base.Awake();
    _networkController = GetComponent<StationNetworkController>();
}

// Update Interact method
public override void Interact(PlayerController player)
{
    if (!IsServer)
    {
        InteractServerRpc(player.NetworkObject);
        return;
    }
    
    // Check if player can use station
    if (_networkController != null && !_networkController.CanPlayerUse(player.OwnerClientId))
    {
        Debug.Log("Station is locked by another player");
        return;
    }
    
    // Lock station
    if (_networkController != null)
    {
        _networkController.RequestUseStationServerRpc(player.OwnerClientId);
    }
    
    // ... existing interaction logic ...
    
    // Release station when done
    if (_networkController != null)
    {
        _networkController.ReleaseStationServerRpc(player.OwnerClientId);
    }
}
```

**Status**: ⬜ Not done

---

### 2. Update PlayerController Network Registration (15 min)
**File**: `Assets/Scripts/Core/Characters/PlayerController.cs`

**Current Issue**: Doesn't register with PlayerNetworkManager

**What to Add**:
```csharp
// In OnNetworkSpawn()
public override void OnNetworkSpawn()
{
    base.OnNetworkSpawn();
    
    // Register with PlayerNetworkManager
    if (IsOwner)
    {
        var services = GameBootstrap.Services;
        if (services?.PlayerNetworkManager != null)
        {
            services.PlayerNetworkManager.RegisterPlayer(OwnerClientId, this);
        }
    }
    
    // ... rest of existing code ...
}

// In OnNetworkDespawn()
public override void OnNetworkDespawn()
{
    base.OnNetworkDespawn();
    
    // Unregister
    if (IsOwner)
    {
        var services = GameBootstrap.Services;
        if (services?.PlayerNetworkManager != null)
        {
            services.PlayerNetworkManager.UnregisterPlayer(OwnerClientId);
        }
    }
}
```

**Status**: ⬜ Not done

---

### 3. Update Ingredient Spawning (20 min)
**File**: `Assets/Scripts/Gameplay/Stations/IngredientSpawner.cs` (or wherever you spawn ingredients)

**Current Issue**: Uses direct Instantiate instead of network spawning

**What to Change**:
```csharp
// OLD CODE (remove this):
GameObject ingredient = Instantiate(ingredientPrefab, position, rotation);

// NEW CODE (use this):
IngredientNetworkSpawner spawner = FindObjectOfType<IngredientNetworkSpawner>();
if (spawner != null && IsServer)
{
    NetworkObject ingredient = spawner.SpawnIngredient(ingredientData, position);
}
```

**Status**: ⬜ Not done

---

### 4. Connect Game Start to Network (15 min)
**File**: Your lobby UI or game start script

**Current Issue**: Doesn't use NetworkGameStateManager

**What to Add**:
```csharp
// In your "Start Game" button handler
public void OnStartGameClicked()
{
    if (NetworkManager.Singleton.IsHost)
    {
        NetworkGameStateManager stateManager = FindObjectOfType<NetworkGameStateManager>();
        if (stateManager != null)
        {
            stateManager.RequestStartGameServerRpc();
        }
    }
}
```

**Status**: ⬜ Not done

---

## 🎮 **Gameplay Features (Needed for Full Functionality)**

### 5. Update ServingStation for Scoring (30 min)
**File**: `Assets/Scripts/Gameplay/Stations/ServingStation.cs`

**Current Issue**: Doesn't use NetworkScoreManager or dish validation

**What to Add**:
```csharp
private NetworkScoreManager _scoreManager;
private IDishValidator _validator;

protected override void Awake()
{
    base.Awake();
    _scoreManager = FindObjectOfType<NetworkScoreManager>();
    _validator = new StandardDishValidator();
}

protected override bool ProcessIngredient(IngredientItem ingredientItem)
{
    if (!IsServer) return false;
    
    // Get plate
    PlateItem plate = ingredientItem.GetComponent<PlateItem>();
    if (plate == null) return false;
    
    // Get recipe
    Recipe recipe = _orderManager.GetRecipeById(plate.RecipeId);
    if (recipe == null) return false;
    
    // Validate dish
    List<IngredientItem> ingredients = plate.GetIngredients();
    if (_validator.ValidateDish(ingredients, recipe))
    {
        // Calculate score
        RoundTimer timer = FindObjectOfType<RoundTimer>();
        float timeRemaining = timer != null ? timer.TimeRemaining : 0f;
        int score = _validator.CalculateScore(ingredients, recipe, timeRemaining);
        
        // Add score
        ulong playerId = ingredientItem.GetComponent<NetworkObject>().OwnerClientId;
        _scoreManager.AddScoreServerRpc(playerId, score, ScoreReason.DishCompleted);
        
        // Complete order
        _orderManager.CompleteOrder(plate.RecipeId);
        
        return true;
    }
    
    return false;
}
```

**Status**: ⬜ Not done

---

### 6. Connect UI to Network Events (30 min)
**File**: `Assets/Scripts/UI/GameplayUIManager.cs` (or your UI script)

**What to Add**:
```csharp
private NetworkScoreManager _scoreManager;
private RoundTimer _roundTimer;
private NetworkGameStateManager _stateManager;

void Start()
{
    // Find managers
    _scoreManager = FindObjectOfType<NetworkScoreManager>();
    _roundTimer = FindObjectOfType<RoundTimer>();
    _stateManager = FindObjectOfType<NetworkGameStateManager>();
    
    // Subscribe to events
    if (_scoreManager != null)
        _scoreManager.OnPlayerScoreUpdated += UpdateScoreUI;
    
    if (_roundTimer != null)
        _roundTimer.OnTimeUpdated += UpdateTimerUI;
    
    if (_stateManager != null)
        _stateManager.OnPhaseChanged += UpdatePhaseUI;
}

void OnDestroy()
{
    // Unsubscribe
    if (_scoreManager != null)
        _scoreManager.OnPlayerScoreUpdated -= UpdateScoreUI;
    if (_roundTimer != null)
        _roundTimer.OnTimeUpdated -= UpdateTimerUI;
    if (_stateManager != null)
        _stateManager.OnPhaseChanged -= UpdatePhaseUI;
}

private void UpdateScoreUI(ulong playerId, int score)
{
    if (playerId == NetworkManager.Singleton.LocalClientId)
    {
        scoreText.text = $"Score: {score}";
    }
}

private void UpdateTimerUI(float timeRemaining)
{
    int minutes = Mathf.FloorToInt(timeRemaining / 60f);
    int seconds = Mathf.FloorToInt(timeRemaining % 60f);
    timerText.text = $"{minutes:00}:{seconds:00}";
}

private void UpdatePhaseUI(GamePhase phase)
{
    phaseText.text = phase.ToString();
}
```

**Status**: ⬜ Not done

---

## 📊 **Integration Status Summary**

### Infrastructure (Code Files)
- ✅ NetworkGameManager - Complete
- ✅ PlayerNetworkManager - Complete
- ✅ NetworkObjectPool - Complete
- ✅ StationNetworkController - Complete
- ✅ NetworkScoreManager - Complete
- ✅ RoundTimer - Complete
- ✅ NetworkGameStateManager - Complete
- ✅ All interfaces and implementations - Complete

### Scene Setup
- ⬜ Run Network Setup Wizard
- ⬜ Validate with Network Scene Validator

### Code Integration
- ⬜ Update CookingStation (30 min)
- ⬜ Update PlayerController (15 min)
- ⬜ Update ingredient spawning (20 min)
- ⬜ Connect game start (15 min)
- ⬜ Update ServingStation (30 min)
- ⬜ Connect UI events (30 min)

### Testing
- ⬜ Test local (host only)
- ⬜ Test multiplayer (host + client)
- ⬜ Test all gameplay features

---

## ⏱️ **Time Estimates**

- **Scene Setup**: 5 minutes (automated)
- **Critical Integration**: 1.5 hours
- **Gameplay Integration**: 1 hour
- **UI Integration**: 30 minutes
- **Testing & Fixes**: 1-2 hours

**Total**: ~4-5 hours of work

---

## 🎯 **Minimum Viable Multiplayer (MVP)**

To get basic multiplayer working, you MUST do:
1. ✅ Run Setup Wizard (5 min)
2. ⬜ Update CookingStation (30 min)
3. ⬜ Update PlayerController (15 min)
4. ⬜ Update ingredient spawning (20 min)

**Total MVP Time**: ~1 hour 10 minutes

After this, players can connect and interact with stations, but scoring/orders won't work yet.

---

## 🚀 **Recommended Approach**

### Day 1: Setup & Critical (1.5 hours)
1. Run Network Setup Wizard
2. Update CookingStation
3. Update PlayerController
4. Update ingredient spawning
5. Test basic multiplayer

### Day 2: Gameplay (1.5 hours)
6. Connect game start/end
7. Update ServingStation
8. Test order completion

### Day 3: Polish (1 hour)
9. Connect UI
10. Add visual feedback
11. Final testing

---

## ✅ **What You Can Do Right Now**

1. **Run the Setup Wizard** (5 min) - This will configure your scene
2. **Test the setup** with Network Validator
3. **Start with CookingStation update** - This is the most critical

Then work through the list above one item at a time.

---

## 🆘 **Need Help?**

Each integration step has code examples in:
- **QUICK_START_GUIDE.md** - Code examples for each integration
- **NETCODE_IMPLEMENTATION_PLAN.md** - Architecture details
- This file - Specific TODO items

---

## 📝 **Bottom Line**

**Infrastructure**: ✅ 100% Complete  
**Integration**: ⬜ 0% Complete (needs ~4-5 hours)  
**Will it work now?**: ❌ No, needs integration first  
**Is it ready to integrate?**: ✅ Yes, all tools provided  

The code is solid and ready - you just need to connect it to your existing gameplay!
