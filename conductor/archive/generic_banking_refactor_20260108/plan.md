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
- [x] Task: Conductor - User Manual Verification 'Phase 2: Implementation & Migration'

## Phase 3: Constants & EOS Backend
- [x] 2bc59fc Task: Define Bank Constants & Refactor
    - [x] Create `BankKeys.cs` with constants for "coins", "gems", etc.
    - [x] Refactor `BankService`, `ShopViewModel`, `CurrencyDisplay`, `LocalDiskBankBackend` to use constants.
- [x] b40cd3b Task: Implement EOSBankBackend
    - [x] Create `EOSBankBackend.cs` implementing `IBankBackend`.
    - [x] Integrate with `EOSCloudStorageProvider` or `PlayerDataStorageService`.
    - [x] Ensure it supports async Load/Save.
- [x] e539545 Task: Update Dependency Injection
- [x] eec398f Task: Fix Assembly Dependencies
    - [x] Create `RecipeRage.Core.asmdef`.
    - [x] Move `IEventBus`, `IUIService`, `GameEvents` to `Core/Shared` to break circular dependency.
    - [x] Move Backends (`EOSBankBackend`, `LocalDiskBankBackend`) to `Core/Banking/Backends` to break circular dependency.
    - [x] Update Assembly References.
- [~] Task: Conductor - User Manual Verification 'Phase 3: Constants & EOS Backend'
