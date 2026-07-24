# Soft-Launch Scope (locked)

**Status:** LOCKED  
**Target:** Guest-only closed/soft launch on EOS Dev → Stage  
**Out of scope for v0.1 soft launch:**
- Google / Facebook / Apple login (UI may show disabled or hidden)
- Hell's Kitchen and Last Plate Standing as ship modes
- Anti-cheat, leaderboards, friends-required flows
- Paid UA scale; store listing may be unlisted/internal

**In scope:**
1. Cold boot → guest login → home
2. Queue Rush Service 2v2 (bots fill if needed for solo smoke)
3. Playable match: prime → fight → deliver → tug-of-war win/loss
4. Results → coin reward → optional rewarded ad (if MAX wired; else hidden)
5. Progress/settings survive relaunch via local + EOS Player Data Storage when online
6. Analytics events fire to Firebase **or** debug sink (document which)
7. No crash on Android + one desktop platform for internal testers

**Version labels:**
- Unity `bundleVersion`: start `0.1.0-soft`
- EOS `ProductVersion`: match Unity marketing version major.minor (`0.1.0`)

**Monetization default:** Ship **without** IAP and **without** interstitial ads unless Task 8 completes. Rewarded post-match is optional nicety, not a gate.
