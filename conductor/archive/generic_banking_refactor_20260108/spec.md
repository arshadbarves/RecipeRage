# Specification: Generic Banking System Refactor

## Overview
Refactor the existing `BankService` to be a fully generic, string-key based persistence system. This allows the game to support an arbitrary number of currencies (Gold, Gems, Energy, PvP Points), inventory items (Skins, Weapons), and progression data (Level, Quest Flags) without modifying the C# codebase.

## Core Components

### 1. BankData (Generic Model)
Instead of:
```csharp
public int Coins;
public int Gems;
public List<string> UnlockedSkins;
```
It will be:
```csharp
public Dictionary<string, long> Balances; // "coins"->100, "gems"->50
public HashSet<string> Inventory;         // "skin_red", "weapon_sword"
public Dictionary<string, string> Data;   // "level"->"5", "tutorial_complete"->"true"
```

### 2. IBankService (Generic Interface)
- **Balances:** `GetBalance(id)`, `SetBalance(id, amount)`, `AddBalance(id, amount)`
- **Inventory:** `HasItem(id)`, `AddItem(id)`, `RemoveItem(id)`
- **Data:** `GetData(key)`, `SetData(key, value)`
- **Transactions:** `Purchase(itemId, cost, currencyId)` -> Atomic check-deduct-grant operation.

## Backwards Compatibility
- The standard IDs "coins", "gems" will be used to maintain compatibility with existing prefabs/UI logic where possible (passed as strings).

## Scalability
- **New Currencies:** Just add a new key in the backend/database. No code change.
- **New Features:** Store "BattlePassLevel" in `Data` dictionary. No code change.
