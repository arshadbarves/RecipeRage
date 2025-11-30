# Matchmaking Screen Theme Solution

## Issues Fixed

### ✅ Issue 1: UIScreenType.Game Not Found

**Problem:** GameplayState was trying to show `UIScreenType.Game` which doesn't exist.

**Solution:** Removed the UI Toolkit screen calls since you're using `GameplayUIManager` (MonoBehaviour-based) for in-game UI.

**Changes Made:**
```csharp
// GameplayState.Enter() - Before
uiService.HideAllScreens(true);
uiService.ShowScreen(UIScreenType.Game, true, false); // ❌ Doesn't exist

// GameplayState.Enter() - After
uiService.HideAllScreens(true); // ✅ Just hide all UI Toolkit screens
// GameplayUIManager handles the in-game UI
```

### ✅ Issue 2: Matchmaking Screen Theme

## Best Solution: Match Your Game's Theme

You have several options for theming the matchmaking screen:

### Option 1: Quick Theme (5 minutes) ⚡
Update the USS file with your game's colors and fonts.

### Option 2: Custom UXML (15 minutes) 🎨
Create a custom layout that matches your game's style.

### Option 3: Use Existing UI System (30 minutes) 🔧
Integrate with your existing GameplayUIManager style.

---

## Option 1: Quick Theme Update (Recommended)

### Step 1: Update Colors in USS

Edit `Assets/Resources/UI/Styles/Screens/MatchmakingScreen.uss`:

```css
/* Match your game's color scheme */
.matchmaking-content {
    flex-grow: 1;
    align-items: center;
    justify-content: center;
    background-color: rgba(20, 20, 30, 0.95); /* Dark blue-ish */
    padding: 40px;
}

/* Status Indicator - Use your game's accent color */
.status-indicator.searching {
    background-color: #FF6B35; /* Orange/Red - change to your color */
    /* Add your game's animation style */
}

.status-indicator.found {
    background-color: #4ECDC4; /* Teal - change to your color */
}

/* Text - Use your game's font */
.status-text {
    font-size: 32px;
    color: #FFFFFF;
    margin-bottom: 20px;
    -unity-text-align: middle-center;
    /* Add your custom font here if you have one */
    /* -unity-font: url('path/to/your/font.ttf'); */
}

/* Player Count - Make it pop */
.player-count {
    font-size: 64px;
    color: #FFD700; /* Gold - change to your accent color */
    font-weight: bold;
    margin-bottom: 10px;
    -unity-text-align: middle-center;
}

/* Cancel Button - Match your game's button style */
.cancel-button {
    width: 200px;
    height: 60px;
    font-size: 24px;
    background-color: #E63946; /* Red - change to your color */
    color: white;
    border-radius: 10px;
    border-width: 2px;
    border-color: #C1121F; /* Darker red */
    transition-duration: 0.2s;
}

.cancel-button:hover {
    background-color: #F1495A;
    scale: 1.05;
}

.cancel-button:active {
    background-color: #C1121F;
    scale: 0.95;
}
```

### Step 2: Add Your Game's Background

```css
.matchmaking-content {
    /* Add background image */
    background-image: url('path/to/your/background.png');
    background-size: cover;
    
    /* Or use gradient */
    background-image: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}
```

---

## Option 2: Custom UXML Layout

If you want a completely custom layout, update `Assets/Resources/UI/Templates/Screens/MatchmakingTemplate.uxml`:

### Example: Card-Based Layout

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements">
    <ui:VisualElement name="matchmaking-screen" class="screen full-screen">
        <ui:VisualElement name="content" class="matchmaking-content">
            
            <!-- Card Container -->
            <ui:VisualElement name="matchmaking-card" class="matchmaking-card">
                
                <!-- Header -->
                <ui:VisualElement name="header" class="card-header">
                    <ui:Label text="FINDING MATCH" class="header-title" />
                </ui:VisualElement>
                
                <!-- Body -->
                <ui:VisualElement name="body" class="card-body">
                    
                    <!-- Status Indicator -->
                    <ui:VisualElement name="status-indicator" class="status-indicator searching" />
                    
                    <!-- Status Text -->
                    <ui:Label name="status-text" text="Searching for players..." class="status-text" />
                    
                    <!-- Player Count (Big) -->
                    <ui:VisualElement name="player-count-container" class="player-count-container">
                        <ui:Label name="player-count" text="1/8" class="player-count" />
                        <ui:Label text="PLAYERS" class="player-label" />
                    </ui:VisualElement>
                    
                    <!-- Search Time -->
                    <ui:Label name="search-time" text="0:00" class="search-time" />
                    
                    <!-- Player List (Optional) -->
                    <ui:ScrollView name="player-list" class="player-list">
                        <!-- Players added dynamically -->
                    </ui:ScrollView>
                    
                </ui:VisualElement>
                
                <!-- Footer -->
                <ui:VisualElement name="footer" class="card-footer">
                    <ui:Button name="cancel-button" text="CANCEL" class="cancel-button" />
                </ui:VisualElement>
                
            </ui:VisualElement>
            
        </ui:VisualElement>
    </ui:VisualElement>
</ui:UXML>
```

### Matching USS for Card Layout

```css
.matchmaking-card {
    width: 600px;
    background-color: rgba(30, 30, 40, 0.95);
    border-radius: 20px;
    border-width: 2px;
    border-color: rgba(255, 255, 255, 0.1);
    overflow: hidden;
}

.card-header {
    background-color: rgba(255, 107, 53, 0.2);
    padding: 20px;
    border-bottom-width: 2px;
    border-bottom-color: rgba(255, 107, 53, 0.5);
}

.header-title {
    font-size: 24px;
    color: #FF6B35;
    font-weight: bold;
    -unity-text-align: middle-center;
}

.card-body {
    padding: 40px;
    align-items: center;
}

.card-footer {
    padding: 20px;
    background-color: rgba(0, 0, 0, 0.3);
    align-items: center;
}
```

---

## Option 3: Match RecipeRage Theme

Based on your game being "RecipeRage" (cooking game), here's a themed version:

### Cooking-Themed Colors

```css
/* RecipeRage Theme */
.matchmaking-content {
    background-color: rgba(255, 248, 240, 0.95); /* Cream/Kitchen color */
    background-image: url('path/to/kitchen-background.png');
}

.status-indicator.searching {
    background-color: #FF6B35; /* Hot/Cooking orange */
}

.status-indicator.found {
    background-color: #4CAF50; /* Fresh/Ready green */
}

.status-text {
    color: #2C3E50; /* Dark text on light background */
    font-size: 32px;
}

.player-count {
    color: #FF6B35; /* Orange accent */
    font-size: 72px;
}

.search-time {
    color: #7F8C8D; /* Gray */
}

.cancel-button {
    background-color: #E74C3C; /* Red (stop cooking) */
    border-radius: 30px; /* Rounded like a plate */
}
```

### Cooking-Themed Text

Update `MatchmakingScreen.cs`:

```csharp
protected override void OnShow()
{
    _searchTime = 0f;

    if (_statusText != null)
    {
        _statusText.text = "Finding chefs..."; // Cooking theme!
    }
    
    // ... rest of code
}
```

---

## Quick Color Palette Suggestions

### Option A: Dark Gaming Theme
```css
Background: #1a1a2e
Primary: #16213e
Accent: #0f3460
Highlight: #e94560
Text: #ffffff
```

### Option B: Vibrant/Fun Theme
```css
Background: #2d3561
Primary: #c05c7e
Accent: #f3826f
Highlight: #ffb961
Text: #ffffff
```

### Option C: Professional/Clean Theme
```css
Background: #f8f9fa
Primary: #495057
Accent: #007bff
Highlight: #28a745
Text: #212529
```

### Option D: RecipeRage/Cooking Theme
```css
Background: #fff8f0 (cream)
Primary: #ff6b35 (hot orange)
Accent: #4caf50 (fresh green)
Highlight: #ffd700 (golden)
Text: #2c3e50 (dark)
```

---

## Implementation Steps

### 1. Choose Your Theme
Pick one of the options above or create your own color scheme.

### 2. Update USS File
Edit `Assets/Resources/UI/Styles/Screens/MatchmakingScreen.uss` with your colors.

### 3. Test in Unity
Run the game and see how it looks.

### 4. Iterate
Adjust colors, sizes, and spacing until it matches your game.

---

## Advanced: Add Animations

### Pulsing Status Indicator (USS)

```css
@keyframes pulse {
    0% { scale: 1; opacity: 1; }
    50% { scale: 1.1; opacity: 0.8; }
    100% { scale: 1; opacity: 1; }
}

.status-indicator.searching {
    /* Note: USS doesn't support @keyframes yet */
    /* Use DOTween in code instead */
}
```

### Pulsing in Code (MatchmakingScreen.cs)

```csharp
protected override void OnShow()
{
    _searchTime = 0f;

    if (_statusText != null)
    {
        _statusText.text = "Searching for players...";
    }

    if (_statusIndicator != null)
    {
        _statusIndicator.RemoveFromClassList("found");
        _statusIndicator.AddToClassList("searching");
        
        // Add pulsing animation with DOTween
        var animationService = GameBootstrap.Services?.AnimationService;
        if (animationService != null)
        {
            animationService.UI.Pulse(_statusIndicator, 1.5f, 1.1f);
        }
    }
}
```

---

## Summary

### ✅ Fixed Issues
1. Removed UIScreenType.Game calls (doesn't exist)
2. GameplayState now just hides UI Toolkit screens
3. GameplayUIManager handles in-game UI

### 🎨 Theme Options
1. **Quick** - Update colors in USS (5 min)
2. **Custom** - Create new UXML layout (15 min)
3. **Themed** - Match RecipeRage cooking theme (30 min)

### 📝 Recommended Next Steps
1. Choose a color palette
2. Update `MatchmakingScreen.uss` with your colors
3. Test in Unity
4. Add animations (optional)
5. Match your game's overall theme

**The system works! Now just make it pretty!** 🎨✨
