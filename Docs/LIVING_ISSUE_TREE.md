# Castlebound — Living Issue Tree
**Last synced:** 2026-03-01 | **Repo:** https://github.com/f1cklepickle/Castlebound

> This doc mirrors GitHub milestone/issue state as a readable progress snapshot.
> It is not the source of truth — GitHub is. Update this after planning sessions or milestone closes.
> To refresh: type "sync issue tree" in Cowork.

---

## Milestone Summary

| Milestone | Purpose | Status |
|-----------|---------|--------|
| P1 — Living Structure: Endless Power | Prove one structure can grow endlessly and visibly | ✅ Complete (8/8) |
| P2 — Castle Responds to Care | Prove the castle subtly assists defense | ✅ Complete (4/4) |
| P3 — Authored Defense Under Pressure | Prove the player is authoring defense, not reacting randomly | 🔄 In Progress (6/18 — 33%) |
| P4 — Castle Memory & Trust | Prove the castle remembers what the player brings back | ⬜ Not Started (0/8) |
| P5 — Meaningful Failure | Teach Castlebound's philosophy through loss | ⬜ Not Started (0/5) |
| P6 — Prototype Lock & Polish | Stop adding systems, make the experience readable and stable | ⬜ Not Started (0/20) |

---

## P1 — Living Structure: Endless Power ✅
**Purpose:** Prove that a single castle structure can grow endlessly, visibly, and meaningfully.
**Must establish:** One upgradable tower OR barrier, repeated upgrades (no cap), linear stat scaling, visual cycling (wrap/cloth/material color), persistence across waves, strength readable at a glance.

- ✅ All 8 issues closed

---

## P2 — Castle Responds to Care ✅
**Purpose:** Prove the castle is not passive and subtly assists defense.
**Must establish:** One automatic castle reaction, triggered by pressure (all barriers broken + enemy count threshold), subtle/non-lethal/non-flashy, no explicit UI explanation.

- ✅ All 4 issues closed

---

## P3 — Authored Defense Under Pressure 🔄 33%
**Purpose:** Prove the player is authoring the defense, not reacting randomly.
**Must establish:** One deliberate pre-wave choice, no mid-wave construction, choice meaningfully affects defense outcome, clear cause → effect across waves.

- ✅ 6 issues closed
- ⬜ #140 feat(input): pc mouse aim — cursor facing and left-click attack
- ⬜ #139 feat(ui): replace death screen space bar prompt with tappable restart button
- ⬜ #138 feat(ui): wire touch actions to existing UI buttons — potion, weapon swap, repair, upgrade panel
- ⬜ #137 feat(input): android touch controls — movement and aim/attack zones
- ⬜ #132 feat(visual): basic ground tileset
- ⬜ #131 feat(world): authored grid-snap placement pass
- ⬜ #121 Balancing Tables (central tuning assets)
- ⬜ +5 additional open (see GitHub for full list)

---

## P4 — Castle Memory & Trust ⬜
**Purpose:** Prove the castle remembers what the player brings back and never surprises them.
**Must establish:** Loadout / Backpack / Castle Inventory separation, backpack clears at wave end, items persist safely in castle inventory, no auto-destruction, clear combat ↔ stewardship boundary.

**Planned issues (from design sessions):**
- ⬜ Castle Inventory (Vault) — count-based permanent storage, out-of-combat only
- ⬜ Field Backpack — slot-based mid-wave inventory, auto-clears at wave end
- ⬜ Loadout System — Weapon Slot A, Weapon Slot B, Potion Slot
- ⬜ Wave End Deposit Logic — Backpack → Castle Inventory transfer
- ⬜ RNG Weapon Instance System — weapons as instances with stats/perks/rarity
- ⬜ Salvage Orders — opt-in auto-dismantle rules
- ⬜ Manual Item Protection (Locks) — player override, immune to all dismantle
- ⬜ Trust & Safety Guardrails — no dismantling during combat or without explicit order

---

## P5 — Meaningful Failure ⬜
**Purpose:** Teach the philosophy of Castlebound through loss.
**Must establish:** Failure screen, waves survived displayed, one non-blaming philosophical line, fast restart flow.

- ⬜ #100 feat(stats): run stats tracking service
- ⬜ #99 feat(ui): run summary stats (kills, damage, repairs, gold)
- ⬜ #98 feat(flow): fast restart flow
- ⬜ #97 feat(ui): philosophical failure line
- ⬜ +1 additional open (see GitHub)

---

## P6 — Prototype Lock & Polish ⬜
**Purpose:** Stop adding systems and make the experience readable and stable.
**Must establish:** Feedback noise reduction, camera clarity, core tuning pass, bug fixes only, no new mechanics.
**Completion definition:** *"This is a complete Castlebound prototype."*

- ⬜ #120 Locked Palette + Palette-based Tints
- ⬜ #119 Auto-wiring (Editor Tool)
- ⬜ #118 Prefab Creator (Editor Tool)
- ⬜ #117 Scene Validator (Editor Tool)
- ⬜ #113 refactor(ui): centralize upgrade button feedback handling
- ⬜ #112 refactor(player): split PlayerController responsibilities
- ⬜ #111 perf(ui): pool upgrade menu rows
- ⬜ #110 chore(input): regenerate PlayerControls.cs from inputactions
- ⬜ #106 feat(player): movement/defense polish pass
- ⬜ #105 feat(art): castle wall/tower/enemy sprite set + basic animations
- ⬜ #104 fix(gameplay): bug fix sweep only (no new mechanics)
- ⬜ #103 chore(balance): core tuning pass
- ⬜ #102 fix(ui): feedback noise reduction sweep
- ⬜ +7 additional open (see GitHub)

---

## Unassigned
- ⬜ #122 Docs — Living Checklists + Decisions Log *(no milestone assigned)*

---

*To update this doc: run "sync issue tree" in Cowork, or manually edit after a planning session.*
