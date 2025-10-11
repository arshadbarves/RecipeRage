# RecipeRage Feature Specification

## Table of Contents
1. [Game Modes](#game-modes)
2. [Lobby System](#lobby-system)
3. [Character System](#character-system)
4. [Shop System](#shop-system)
5. [Power-up System](#power-up-system)
6. [Monetization](#monetization)

## Game Modes

### Classic Mode
- **Description**: Traditional cooking competition mode
- **Player Count**: 2-4 players
- **Time Limit**: 5 minutes per round
- **Scoring**:
  - Completed orders: +100 points
  - Perfect timing: +50 points bonus
  - Failed orders: -50 points
  - Time penalties: -10 points per second late

### Time Attack Mode
- **Description**: Race against the clock
- **Player Count**: 1-2 players
- **Time Limit**: 3 minutes
- **Mechanics**:
  - Completing orders adds time
  - Failed orders reduces time
  - Game ends when time runs out

### Team Battle Mode
- **Description**: Team-based competition
- **Player Count**: 4-8 players (2-4 per team)
- **Time Limit**: 10 minutes
- **Team Mechanics**:
  - Shared resources
  - Team score tracking
  - Team power-ups

## Lobby System

### Lobby Features
- Player slots (2-8 depending on mode)
- Character/skin selection
- Team assignment (for team mode)
- Ready status tracking
- Chat system
- Game mode selection
- Map voting

### Matchmaking Rules
- Skill-based matching
- Region-based prioritization
- Team balancing
- Party size consideration
- Connection quality filtering

### Session Management
- Auto-kick on AFK
- Reconnection grace period
- Vote kick system
- Host migration
- Session persistence

## Character System

### Base Characters
1. **Speed Chef**
   - Higher movement speed
   - Faster ingredient gathering
   - Lower carrying capacity

2. **Power Chef**
   - Higher carrying capacity
   - Slower movement speed
   - Bonus points for perfect dishes

3. **Tech Chef**
   - Special ability cooldown reduction
   - Average stats
   - Bonus for using equipment

4. **Master Chef**
   - Balanced stats
   - Leadership bonus for team mode
   - Extra recipe slots

### Skin System
- Base skin for each character
- Rare skins with particle effects
- Legendary skins with custom animations
- Event-specific limited skins
- Achievement-based skins

### Character Progression
- Experience points per match
- Character-specific challenges
- Unlockable abilities
- Mastery levels
- Prestige system

## Shop System

### Currency Types
1. **Coins**
   - Earned through gameplay
   - Used for basic items
   - Daily login rewards

2. **Gems**
   - Premium currency
   - Purchased with real money
   - Earned through achievements
   - Special event rewards

### Shop Categories
1. **Character Shop**
   - New characters
   - Character skins
   - Character upgrades

2. **Power-up Shop**
   - Temporary boosts
   - Permanent upgrades
   - Special abilities

3. **Cosmetic Shop**
   - Emotes
   - Effects
   - Kitchen decorations

### Purchase Flow
1. Item selection
2. Currency verification
3. Purchase confirmation
4. Item delivery
5. Purchase history
6. Refund policy

## Power-up System

### Temporary Power-ups
1. **Speed Boost**
   - Duration: 30 seconds
   - Effect: +50% movement speed
   - Cooldown: 60 seconds

2. **Multi-Task**
   - Duration: 15 seconds
   - Effect: Carry multiple ingredients
   - Cooldown: 90 seconds

3. **Perfect Cook**
   - Duration: 10 seconds
   - Effect: Auto-perfect timing
   - Cooldown: 120 seconds

### Permanent Upgrades
1. **Kitchen Mastery**
   - Levels 1-10
   - Each level: +5% cooking speed
   - Cost increases per level

2. **Ingredient Expert**
   - Levels 1-5
   - Each level: +1 ingredient capacity
   - Unlocks special recipes

3. **Time Management**
   - Levels 1-3
   - Each level: +10 seconds per round
   - Reduces time penalties

## Monetization

### Ad Integration
1. **Rewarded Ads**
   - Double coins after match
   - Free power-up
   - Extra time in time attack
   - Daily bonus multiplier

2. **Interstitial Ads**
   - Between matches
   - After shop purchases
   - Menu transitions
   - Configurable frequency

### In-App Purchases
1. **Gem Packages**
   - Starter Pack: 500 gems
   - Popular Pack: 1200 gems
   - Value Pack: 2500 gems
   - Whale Pack: 6500 gems

2. **Special Bundles**
   - Character + Skin bundles
   - Season Pass
   - Event packages
   - Limited time offers

### Season Pass
- Duration: 3 months
- 100 levels
- Free tier rewards
- Premium tier rewards
- Bonus challenges
- Exclusive content

## Implementation Notes

### Priority Features
1. Core gameplay mechanics
2. Basic character system
3. Essential shop features
4. Fundamental power-ups
5. Basic monetization

### Technical Considerations
- Scalable character system
- Efficient power-up management
- Secure transaction handling
- Optimized asset loading
- Anti-cheat measures

### Future Expansions
- New game modes
- Additional characters
- Seasonal events
- Social features
- Tournament system 