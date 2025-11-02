# Quick Start Guide - Netcode Integration

## 🚀 5-Minute Setup

### Step 1: Add Components to Scene (2 minutes)

In your **Game** scene, create a new GameObject called "NetworkManagers" and add these components:

```
NetworkManagers (GameObject)
├── NetworkGameStateManager
├── NetworkScoreManager
├── RoundTimer
└── IngredientNetworkSpawner
```

### Step 2: Configure NetworkManager (1 minute)

1. Select your NetworkManager GameObject
2. Add these prefabs to the **NetworkPrefabs** list:
   - Player prefab
   - Ingredient prefab
   - All station prefabs
   - Plate prefab

### Step 3: Update Station Scripts (2 minutes)

Add `StationNetworkController` component to all station prefabs:
- CookingPot
- CuttingStation
- AssemblyStation
- ServingStation

## 📝 Code Integration Examples

### Example 1: Start Game from Lobby

```csharp
// In your LobbyUI or LobbyState
public class LobbyUI : MonoBehaviour
{
    private NetworkGameStateManager _stateManager;
    
    void Start()
    {
        _stateManager = FindObjectOfType<NetworkGameStateManager>();
    }
    
    public void OnStartGameButtonClicked()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            _stateManager.RequestStartGameServerRpc();
        }
    }
}
```

### Example 2: Spawn Ingredient

```csharp
// In your IngredientSpawner station
public class IngredientSpawner : CookingStation
{
    [SerializeField] private Ingredient _ingredientData;
    private IngredientNetworkSpawner _spawner;
    
    void Awake()
    {
        _spawner = FindObjectOfType<IngredientNetworkSpawner>();
    }
    
    public override void Interact(PlayerController player)
    {
        if (!IsServer) return;
        
        // Spawn ingredient
        NetworkObject ingredient = _spawner.SpawnIngredient(
            _ingredientData, 
            transform.position + Vector3.up
        );
        
        // Give to player
        if (ingredient != null)
        {
            player.PickUpObject(ingredient.gameObject);
        }
    }
}
```

### Example 3: Complete Order and Add Score

```csharp
// In your ServingStation
public class ServingStation : CookingStation
{
    private NetworkScoreManager _scoreManager;
    private OrderManager _orderManager;
    
    void Awake()
    {
        _scoreManager = FindObjectOfType<NetworkScoreManager>();
        _orderManager = FindObjectOfType<OrderManager>();
    }
    
    protected override bool ProcessIngredient(IngredientItem ingredientItem)
    {
        if (!IsServer) return false;
        
        // Get plate from ingredient
        PlateItem plate = ingredientItem.GetComponent<PlateItem>();
        if (plate == null) return false;
        
        // Validate dish
        IDishValidator validator = new StandardDishValidator();
        Recipe recipe = _orderManager.GetRecipeById(plate.RecipeId);
        List<IngredientItem> ingredients = plate.GetIngredients();
        
        if (validator.ValidateDish(ingredients, recipe))
        {
            // Calculate score
            float timeRemaining = FindObjectOfType<RoundTimer>().TimeRemaining;
            int score = validator.CalculateScore(ingredients, recipe, timeRemaining);
            
            // Add score
            ulong playerId = ingredientItem.GetComponent<NetworkObject>().OwnerClientId;
            _scoreManager.AddScoreServerRpc(
                playerId, 
                score, 
                ScoreReason.DishCompleted
            );
            
            // Complete order
            _orderManager.CompleteOrder(plate.RecipeId);
            
            return true;
        }
        
        return false;
    }
}
```

### Example 4: Update UI with Network Events

```csharp
// In your GameplayUIManager
public class GameplayUIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private TextMeshProUGUI _timerText;
    [SerializeField] private TextMeshProUGUI _phaseText;
    
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
        _scoreManager.OnPlayerScoreUpdated += UpdateScore;
        _roundTimer.OnTimeUpdated += UpdateTimer;
        _stateManager.OnPhaseChanged += UpdatePhase;
    }
    
    void OnDestroy()
    {
        // Unsubscribe
        if (_scoreManager != null)
            _scoreManager.OnPlayerScoreUpdated -= UpdateScore;
        if (_roundTimer != null)
            _roundTimer.OnTimeUpdated -= UpdateTimer;
        if (_stateManager != null)
            _stateManager.OnPhaseChanged -= UpdatePhase;
    }
    
    private void UpdateScore(ulong playerId, int score)
    {
        // Update score display
        if (playerId == NetworkManager.Singleton.LocalClientId)
        {
            _scoreText.text = $"Score: {score}";
        }
    }
    
    private void UpdateTimer(float timeRemaining)
    {
        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        _timerText.text = $"{minutes:00}:{seconds:00}";
    }
    
    private void UpdatePhase(GamePhase phase)
    {
        _phaseText.text = phase.ToString();
    }
}
```

### Example 5: Enhanced Station with Network Controller

```csharp
// Update your CookingStation base class
public abstract class CookingStation : NetworkBehaviour, IInteractable
{
    protected StationNetworkController _networkController;
    
    protected override void Awake()
    {
        base.Awake();
        _networkController = GetComponent<StationNetworkController>();
    }
    
    public virtual void Interact(PlayerController player)
    {
        if (!IsServer)
        {
            InteractServerRpc(player.NetworkObject);
            return;
        }
        
        // Check if player can use station
        if (!_networkController.CanPlayerUse(player.OwnerClientId))
        {
            Debug.Log("Station is locked by another player");
            return;
        }
        
        // Lock station for this player
        _networkController.RequestUseStationServerRpc(player.OwnerClientId);
        
        // Continue with existing interaction logic...
        
        // Release station when done
        _networkController.ReleaseStationServerRpc(player.OwnerClientId);
    }
}
```

## 🧪 Testing Steps

### 1. Test Locally (Host Only)
```
1. Start Unity
2. Play scene
3. Click "Start as Host" in your lobby
4. Verify player spawns
5. Test ingredient pickup/drop
6. Test station interactions
7. Test order completion
8. Check score updates
9. Watch timer countdown
```

### 2. Test Multiplayer (Host + Client)
```
1. Build the game
2. Run build as Host
3. Run Unity Editor as Client
4. Connect client to host
5. Verify both players spawn
6. Test simultaneous interactions
7. Verify score syncs to both
8. Verify timer syncs to both
9. Test disconnection handling
```

## 🔧 Common Issues & Solutions

### Issue: Player doesn't spawn
**Solution**: Ensure Player prefab is in NetworkManager's NetworkPrefabs list

### Issue: Ingredients don't sync
**Solution**: Ensure Ingredient prefab has NetworkObject component and is in NetworkPrefabs list

### Issue: Stations don't lock
**Solution**: Ensure StationNetworkController component is added to station prefabs

### Issue: Scores don't update
**Solution**: Ensure NetworkScoreManager is in the scene and spawned

### Issue: Timer doesn't sync
**Solution**: Ensure RoundTimer is in the scene and spawned

### Issue: "Only server can..." warnings
**Solution**: Wrap server-only code in `if (!IsServer) return;` checks

## 📊 Performance Tips

### 1. Prewarm Object Pool
```csharp
// In your game initialization
void Start()
{
    if (NetworkManager.Singleton.IsServer)
    {
        var pool = GameBootstrap.Services.NetworkObjectPool;
        pool.Prewarm(ingredientPrefab, 20); // Prewarm 20 ingredients
    }
}
```

### 2. Batch Score Updates
```csharp
// Instead of multiple AddScore calls
// Batch them if possible
int totalScore = baseScore + timeBonus + comboBonus;
_scoreManager.AddScoreServerRpc(playerId, totalScore, ScoreReason.DishCompleted);
```

### 3. Use Unreliable Delivery for Visual Effects
```csharp
[ClientRpc(Delivery = RpcDelivery.Unreliable)]
private void PlayEffectClientRpc()
{
    // Non-critical visual effect
}
```

## 🎯 Next Steps

1. ✅ Complete scene setup
2. ✅ Test locally
3. ✅ Test multiplayer
4. ⬜ Add visual feedback for network events
5. ⬜ Implement reconnection handling
6. ⬜ Add network statistics display
7. ⬜ Profile network bandwidth
8. ⬜ Optimize for mobile

## 📚 Reference

- Full documentation: `NETCODE_IMPLEMENTATION_PLAN.md`
- Implementation summary: `IMPLEMENTATION_SUMMARY.md`
- Architecture patterns: `../../../.kiro/steering/patterns.md`

## 🆘 Need Help?

Check these files for detailed information:
- Network architecture: `NETCODE_IMPLEMENTATION_PLAN.md`
- SOLID principles: `../../../.kiro/steering/patterns.md`
- Project structure: `../../../.kiro/steering/structure.md`
- Technology stack: `../../../.kiro/steering/tech.md`
