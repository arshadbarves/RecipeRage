# Map System Guide for Level Designers

## Overview

RecipeRage uses an **additive scene loading system** for maps. This means:
- One base scene (`Game.unity`) contains all game systems
- Individual map scenes are loaded on top additively
- Each map can have unique lighting, environment, and visual style

## Quick Start

### 1. Creating a New Map

#### Step 1: Duplicate the Base Map Scene
1. Navigate to `Assets/Scenes/`
2. Find an existing map scene (e.g., `Map_Kitchen.unity`)
3. Duplicate it: Right-click → Duplicate
4. Rename it following the naming convention: `Map_YourMapName.unity`
   - Examples: `Map_Restaurant.unity`, `Map_Factory.unity`, `Map_Rooftop.unity`

#### Step 2: Design Your Map
Open your new map scene and design it:

**What to Include:**
- ✅ Environment meshes (floors, walls, props)
- ✅ Station positions (prep stations, cooking stations, delivery)
- ✅ Spawn points for players
- ✅ Lighting (Directional Light, point lights, area lights)
- ✅ Post-processing volumes (fog, bloom, color grading)
- ✅ NavMesh (for AI pathfinding if needed)
- ✅ Collision meshes
- ✅ Visual effects (particles, ambient effects)

**What NOT to Include:**
- ❌ GameplayLifetimeScope (stays in Game.unity)
- ❌ NetworkManager (stays in Game.unity)
- ❌ OrderManager (stays in Game.unity)
- ❌ ScoreManager (stays in Game.unity)
- ❌ Any gameplay logic scripts

**Important:** Your map scene should ONLY contain the environment and visual elements.

#### Step 3: Setup Lighting
1. Window → Rendering → Lighting
2. Configure your lighting settings:
   - Skybox Material (unique per map)
   - Ambient lighting color
   - Fog settings (color, density)
3. Add lights to your scene
4. **Generate Lighting** → Click "Generate Lighting" to bake lightmaps

**Pro Tip:** Each map can have completely different lighting moods:
- Kitchen: Bright, warm lighting
- Restaurant: Dim, elegant lighting
- Factory: Industrial, cool lighting

### 2. Creating Map Data Asset

#### Step 1: Create MapData ScriptableObject
1. In Project window, navigate to `Assets/Resources/Maps/`
2. Right-click → Create → RecipeRage → Map Data
3. Name it: `YourMapNameData` (e.g., `KitchenMapData.asset`)

#### Step 2: Configure MapData
Select your MapData asset and fill in the fields:

| Field | Description | Example |
|-------|-------------|---------|
| **Id** | Unique identifier (auto-fills from Scene Name) | `Map_Kitchen` |
| **Display Name** | Name shown in UI | `Kitchen Arena` |
| **Description** | Short description for players | `A classic kitchen battleground with tight corridors` |
| **Preview Image** | Screenshot/thumbnail for map selection | Drag your preview sprite here |
| **Scene Name** | MUST match your scene file name exactly | `Map_Kitchen` |
| **Recommended Player Count** | Ideal number of players | `4` |
| **Map Size** | Small/Medium/Large | `Medium` |
| **Theme** | Visual theme/style | `Kitchen` |

**Critical:** The `Scene Name` field MUST exactly match your scene file name (without .unity extension).

### 3. Integrating with Game Modes

#### Step 1: Open GameMode Asset
1. Navigate to `Assets/Resources/GameModes/`
2. Open an existing GameMode asset (e.g., `ClassicMode.asset`)

#### Step 2: Add Your Map
In the Inspector:
1. Find **Map Settings** section
2. Add your map to `Available Maps` list:
   - Click the `+` button
   - Type your scene name: `Map_Kitchen`
3. Set `Default Map` if this should be the default: `Map_Kitchen`

**Example Configuration:**
```
Game Mode: Classic Mode
├── Available Maps:
│   ├── Map_Kitchen
│   ├── Map_Restaurant
│   └── Map_Factory
└── Default Map: Map_Kitchen
```

### 4. Adding to Build Settings

**Important:** Unity needs to know about your scene to load it.

1. File → Build Settings
2. Click "Add Open Scenes" (with your map scene open)
3. Or drag your scene file into the "Scenes In Build" list

Your build settings should look like:
```
Scenes In Build:
✓ Bootstrap (index 0)
✓ MainMenu (index 1)
✓ Game (index 2)
✓ Map_Kitchen (index 3)
✓ Map_Restaurant (index 4)
✓ Map_Factory (index 5)
```

## Map Design Guidelines

### Layout Recommendations

#### Small Maps (2-4 players)
- Compact layout: 20x20 to 30x30 units
- 2-3 cooking stations
- Short travel distances
- High intensity gameplay

#### Medium Maps (4-6 players)
- Balanced layout: 30x40 to 40x50 units
- 4-5 cooking stations
- Multiple paths between areas
- Moderate pacing

#### Large Maps (6-8 players)
- Spacious layout: 50x60+ units
- 6+ cooking stations
- Complex routing options
- Strategic positioning important

### Station Placement
- **Prep Stations:** Near ingredient spawns
- **Cooking Stations:** Central locations
- **Delivery Stations:** Clearly marked, accessible from multiple routes
- **Spacing:** Minimum 3-5 units between stations to avoid overcrowding

### Player Spawn Points
- Place spawn points evenly around the map
- Keep distance from active cooking areas
- Consider team-based spawning (if applicable)
- Test that players don't spawn inside geometry

### Visual Clarity
- **Color Coding:** Use consistent colors for station types
- **Lighting:** Ensure all gameplay areas are well-lit
- **Contrast:** Important objects should stand out from background
- **Accessibility:** Clear paths, no confusing layouts

## Testing Your Map

### Solo Testing
1. Open `Game.unity` scene
2. Select the GameMode you configured
3. Enter Play Mode
4. Your map should load additively on top of Game.unity

### Multiplayer Testing
1. Build the game with your new map included
2. Create a lobby with the GameMode that includes your map
3. Ensure all players can see and interact with the map properly

### Performance Testing
Check the following:
- **Frame Rate:** Should maintain 60 FPS on target hardware
- **Draw Calls:** Keep under 1000 for mobile, 2000 for PC
- **Lighting:** Baked lighting loads quickly
- **Collisions:** No stuck spots or collision issues

## Common Issues

### "Scene not found" Error
**Problem:** Map scene name doesn't match MapData scene name
**Solution:**
1. Check your scene file name in Project window
2. Check MapData `Scene Name` field
3. They must match exactly (case-sensitive)

### Map doesn't load
**Problem:** Scene not in Build Settings
**Solution:** File → Build Settings → Add your scene

### Lighting looks wrong
**Problem:** Lightmaps not baked or missing
**Solution:**
1. Open your map scene
2. Window → Rendering → Lighting
3. Click "Generate Lighting"
4. Save scene

### GameplayLifetimeScope missing error
**Problem:** You removed it from Game.unity or added it to map scene
**Solution:** GameplayLifetimeScope should ONLY be in `Game.unity`, never in map scenes

### Stations don't work
**Problem:** Station references broken
**Solution:**
1. Stations should be in your map scene
2. GameplayLifetimeScope in Game.unity will find them automatically
3. Ensure stations have proper scripts attached

## Advanced Features

### Post-Processing Profiles
Each map can have unique post-processing:
1. Create new Post-Processing Profile: Right-click → Create → Post-Processing Profile
2. In your map scene, add Post-Processing Volume component
3. Assign your profile
4. Configure effects (bloom, color grading, vignette, etc.)

### Custom Skyboxes
1. Create/import skybox material
2. Window → Rendering → Lighting
3. Assign to Skybox Material
4. This is per-scene, so each map can have unique sky

### Audio Ambience
Add map-specific ambient sounds:
1. Add empty GameObject: "MapAudio"
2. Add Audio Source components
3. Assign ambient sound clips
4. Set to Play On Awake, Loop enabled
5. Configure 3D spatial blend as needed

### Dynamic Elements
You can add map-specific dynamic elements:
- Moving platforms
- Hazards (fire, water, etc.)
- Interactive props
- Time-of-day changes
- Weather effects

Just ensure they don't rely on GameplayLifetimeScope services.

## Checklist for New Maps

Before submitting your map, verify:

- [ ] Scene named with `Map_` prefix
- [ ] MapData asset created in `Assets/Resources/Maps/`
- [ ] Scene Name in MapData matches scene file name exactly
- [ ] Display Name and Description filled in
- [ ] Preview image added
- [ ] Lighting baked (no errors in console)
- [ ] Scene added to Build Settings
- [ ] Added to at least one GameMode's Available Maps
- [ ] Player spawn points placed and tested
- [ ] All stations functional and accessible
- [ ] No GameplayLifetimeScope in map scene
- [ ] Performance tested (60 FPS maintained)
- [ ] Collision tested (no stuck spots)
- [ ] Multiplayer tested (if applicable)

## File Structure Reference

```
RecipeRage/
├── Assets/
│   ├── Scenes/
│   │   ├── Bootstrap.unity          (Entry point - don't touch)
│   │   ├── MainMenu.unity           (Main menu - don't touch)
│   │   ├── Game.unity               (Base gameplay - don't touch)
│   │   ├── Map_Kitchen.unity        (Your map scenes)
│   │   ├── Map_Restaurant.unity     (Your map scenes)
│   │   └── Map_Factory.unity        (Your map scenes)
│   │
│   └── Resources/
│       ├── Maps/                     (MapData assets)
│       │   ├── KitchenMapData.asset
│       │   ├── RestaurantMapData.asset
│       │   └── FactoryMapData.asset
│       │
│       └── GameModes/                (GameMode assets)
│           ├── ClassicMode.asset
│           └── TimeAttackMode.asset
```

## Getting Help

### Unity Console Errors
Check the Console window (Window → General → Console) for errors related to:
- Scene loading
- Missing references
- MapLoader messages

### Log Messages
The system logs useful information:
```
[GameplayState] Loading map: Map_Kitchen
[MapLoader] Map loaded successfully: Map_Kitchen
```

### Contact
If you encounter issues:
1. Check this guide first
2. Verify the checklist above
3. Check Unity Console for errors
4. Contact the programming team with:
   - Map name
   - Error message (screenshot of Console)
   - Steps to reproduce

## Example Workflow

Here's a complete workflow for creating a new map:

1. **Planning** (30 min)
   - Sketch layout on paper
   - Decide theme and visual style
   - Plan station locations

2. **Scene Creation** (1-2 hours)
   - Duplicate existing map scene
   - Rename to `Map_YourName.unity`
   - Block out layout with primitives
   - Place station locations

3. **Art Pass** (2-4 hours)
   - Replace primitives with final assets
   - Add props and details
   - Setup materials

4. **Lighting** (1-2 hours)
   - Add lights
   - Configure skybox
   - Bake lightmaps
   - Add post-processing

5. **Configuration** (15 min)
   - Create MapData asset
   - Fill in all fields
   - Add to GameMode(s)
   - Add to Build Settings

6. **Testing** (30-60 min)
   - Solo playtest
   - Check performance
   - Fix issues
   - Multiplayer test

7. **Polish** (1-2 hours)
   - Add audio ambience
   - Final visual tweaks
   - Performance optimization
   - Final testing

**Total Time:** Approximately 6-12 hours per map (varies by complexity)

---

## Version History
- **v1.0** (2026-01-06) - Initial documentation
