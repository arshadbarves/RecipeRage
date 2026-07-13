# Drift Warning Protocol

This page defines the procedure for detecting and handling design drift — when implementation
or a proposed change contradicts what is documented in this wiki.

---

## What Is Drift?

Drift occurs when:

1. **Code contradicts wiki** — an implemented feature works differently from what the wiki specifies
2. **Proposal contradicts wiki** — a new design idea conflicts with existing documented decisions
3. **Wiki contradicts wiki** — two wiki pages describe the same concept differently
4. **Scope creep** — a task expands beyond what the wiki documents without acknowledgement

---

## The Drift Warning Format

When an LLM agent or developer detects drift, they MUST issue a warning in this format before proceeding:

```
⚠️  DRIFT WARNING
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Wiki says:      [exact quote or summary of what the wiki specifies]
                Source: wiki/[Page].md

You are proposing: [description of the change or conflict]

Impact:         [what breaks or changes if we proceed]

Options:
  A) Keep wiki — revert/adjust the change to match the wiki
  B) Update wiki — confirm this is intentional, update wiki to match
  C) Investigate — need more context before deciding

What would you like to do? (A / B / C)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```

---

## When to Issue a Drift Warning

| Scenario | Issue Warning? |
|----------|---------------|
| Implementation matches wiki exactly | No |
| Minor code refactor, same behaviour | No |
| New feature not yet in wiki | No — just update wiki after |
| Changing a behaviour already documented in wiki | **YES** |
| Removing a feature documented in wiki | **YES** |
| Adding a mechanic that contradicts a documented rule | **YES** |
| Changing architecture (DI scope, layer, service ownership) | **YES** |
| Changing scoring formula, trophy values, or RC key defaults | **YES** |
| Control scheme change | **YES** |
| New game mode that conflicts with documented modes | **YES** |

---

## How to Update the Wiki After Confirmation

When the user confirms option B (update wiki):

1. Edit the relevant wiki page(s) to reflect the new decision
2. Add a `> **Updated [date]:** [brief description of change]` note under the changed section
3. Append an entry to `wiki/log.md` in this format:

```markdown
## [YYYY-MM-DD] drift-resolved | [short description]

- Drift detected in: [wiki page(s)]
- Original spec: [brief]
- New decision: [brief]
- Confirmed by: user
- Updated pages: [list]
```

---

## Drift Severity Levels

| Level | Description | Example |
|-------|-------------|---------|
| 🔴 Critical | Contradicts a core architecture rule | Using static singletons, Firebase Auth, MonoBehaviour in Domain layer |
| 🟠 Major | Changes documented design behaviour | Scoring formula change, new control mapping, mode rule change |
| 🟡 Minor | Extends or elaborates documented behaviour | Adding a new RC key, new chef, new map variant |
| 🟢 Additive | Completely new area not in wiki | New feature not mentioned anywhere in wiki |

Critical and Major drifts always require explicit user confirmation.  
Minor drifts can be noted with a brief inline comment.  
Additive changes just need the wiki updated after the fact.

---

## Wiki Pages That Trigger Drift Warnings Most Often

| Page | High-Drift Areas |
|------|-----------------|
| [LLM-Rules.md](LLM-Rules.md) | Forbidden patterns, MonoBehaviour locations, DI scope rules |
| [Gameplay.md](Gameplay.md) | Scoring formula, RC key defaults, match duration |
| [GameplayDesign.md](GameplayDesign.md) | Mode rules, serving mechanics, combo system |
| [Characters.md](Characters.md) | Ability system interface, chef roster |
| [Technical.md](Technical.md) | VContainer scope tree, networking rules |
| [Monetization.md](Monetization.md) | Ad rules (NO ads during match), Battle Pass perks |

---

## Example: Scoring Formula Drift

```
⚠️  DRIFT WARNING
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Wiki says:      Tier 2 multiplier = 1.5× (score_tier2_mult default)
                Tier 3 multiplier = 2.0× (score_tier3_mult default)
                Source: wiki/Gameplay.md — Score Events table

You are proposing: Change Tier 3 multiplier to 3.0× to make high-tier dishes
                   more worth attempting.

Impact:         Max match score would rise from ~150pts to ~200pts.
                Trophy balance tuned to 80–150pt range may need re-tuning.
                All score_* RC keys still work — just different defaults.

Options:
  A) Keep wiki — keep Tier 3 at 2.0×
  B) Update wiki — confirm 3.0× is the new value, update Gameplay.md + RC key table
  C) Investigate — run playtests at both values first

What would you like to do? (A / B / C)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```

---

## Source

This protocol was established on 2026-06-02 as part of the gameplay redesign initiative.
