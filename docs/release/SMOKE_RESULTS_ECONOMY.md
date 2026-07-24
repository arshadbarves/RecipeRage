# Smoke: Results economy

## Steps
1. Finish Rush match.
2. Results shows win/loss.
3. Wallet coins increase once.
4. Analytics debug sink or Firebase shows wallet_credit / match event.
5. Re-enter results (if possible) does not double pay.

## Verification audit (2026-07-24)
- `ResultsPhase.Enter` publishes `MatchEndedEvent` once per entry (`_publishedMatchEnded` guard, reset in `Exit`). Never mints wallet.
- `MatchRewardHandler` (session entry point) is the sole credit path via `IWalletLedger.Credit`. Publishes `MatchRewardEvent` + analytics `WalletCredit`.
- `ResultsViewModel` calls `IEconomyService.AddGems(3)` ONLY in the rewarded-ad success path (gated by `CanShowRewardedAd` + `_adClaimed`). No match coins minted in UI.
- Existing EditMode tests cover: win credit, loss credit, score-bonus rate, reward event publish, zero-credit no-op (`EconomyWalletBridgeTests`). Double-entry is structurally prevented by the once-guard; a re-entered `ResultsPhase.Enter` after `Exit` would re-publish by design (new match → new reward).

## Last run
- Date: _pending — requires Unity Editor run_
- Result: _PENDING_
