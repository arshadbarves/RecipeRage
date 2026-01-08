# Implementation Plan - Generic Banking System Refactor

## Phase 1: Data & Interface Design [checkpoint: 11deb60]
- [x] b4acdbd Task: Design Generic BankData
    - [x] Refactor `BankData.cs` to replace hardcoded fields with Dictionaries:
        - [x] `Dictionary<string, long> Balances` (for Coins, Gems, XP, etc.)
        - [x] `HashSet<string> Inventory` (for Skins, Items)
        - [x] `Dictionary<string, string> Progression` (for generic Key-Value storage like "Level", "MissionStatus")
- [x] ec0bed3 Task: Redefine IBankService
    - [x] Remove specific properties (`Coins`, `Gems`) and methods (`AddCoins`, `UnlockSkin`).
    - [x] Add generic methods:
        - [x] `long GetBalance(string currencyId)`
        - [x] `void ModifyBalance(string currencyId, long amount)`
        - [x] `bool HasItem(string itemId)`
        - [x] `void AddItem(string itemId)`
        - [x] `string GetData(string key)` / `void SetData(string key, string value)`
    - [x] Add Transactional Method:
        - [x] `bool Purchase(string itemId, long cost, string currencyId)`
- [x] 11deb60 Task: Conductor - User Manual Verification 'Phase 1: Data & Interface Design'

## Phase 2: Implementation & Migration
- [x] 530ed99 Task: Implement Generic BankService
    - [x] Implement new generic logic in `BankService.cs`.
    - [x] Ensure `Purchase` method handles balance checks, deduction, and item granting atomically.
    - [x] Implement generic Events (`OnBalanceChanged(id, amount)`, `OnItemUnlocked(id)`).
- [x] 8e64c50 Task: Migrate Consumers
    - [x] Update `SessionLifetimeScope` (already updated in previous track).
    - [x] Refactor `CurrencyDisplay.cs` to listen for generic events and fetch specific IDs ("coins", "gems").
    - [x] Refactor `ShopViewModel.cs` to use `Purchase()`.
    - [x] Refactor `UsernameViewModel.cs` to use `ModifyBalance("gems", -cost)`.
    - [x] Refactor `SkinsService` (internal logic updated in previous track to use IBankService).
- [~] Task: Conductor - User Manual Verification 'Phase 2: Implementation & Migration'
