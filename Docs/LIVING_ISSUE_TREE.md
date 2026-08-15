# Castlebound — Living Issue Tree
**Last synced:** 2026-08-15 | **Repo:** https://github.com/f1cklepickle/Castlebound

> This doc mirrors GitHub milestone/issue state as a readable progress snapshot.
> It is not the source of truth — GitHub is. Update this after planning sessions or milestone closes.
> To refresh: type "sync issue tree" in Cowork.

---

## Milestone Summary

| Milestone | Purpose | Status |
|-----------|---------|--------|
| P1 — Living Structure: Endless Power | Prove one structure can grow endlessly and visibly | ✅ Complete (8/8) |
| P2 — Castle Responds to Care | Prove the castle subtly assists defense | ✅ Complete (4/4) |
| P3 — Authored Defense Under Pressure | Prove the player is authoring defense, not reacting randomly | ✅ Complete (47/47) |
| P4 — Castle Memory & Trust | Prove the castle remembers what the player brings back | ✅ Complete (14/14) |
| P5 — Meaningful Failure | Teach Castlebound's philosophy through loss | ✅ Complete (5/5) |
| P6 — Prototype Content Lock | Lock the remaining combat and defensive-choice feature set | 🟨 In Progress (13/19, 68%) |
| P7 — Prototype Lock & Polish | Make the locked prototype readable, stable, and demonstrable | 🟨 In Progress (16/28, 57%) |

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

## P3 — Authored Defense Under Pressure ✅
**Purpose:** Prove the player is authoring the defense, not reacting randomly.
**Must establish:** One deliberate pre-wave choice, no mid-wave construction, choice meaningfully affects defense outcome, clear cause → effect across waves.

- ✅ All 47 issues closed

---

## P4 — Castle Memory & Trust ✅
**Purpose:** Prove the castle remembers what the player brings back and never surprises them.
**Must establish:** Loadout / Backpack / Castle Inventory separation, backpack clears at wave end, items persist safely in castle inventory, no auto-destruction, clear combat ↔ stewardship boundary.

- ✅ All 14 issues closed

---

## P5 — Meaningful Failure ✅
**Purpose:** Teach the philosophy of Castlebound through loss.
**Must establish:** Failure screen, waves survived displayed, one non-blaming philosophical line, fast restart flow.

- ✅ All 5 issues closed

---

## P6 — Prototype Content Lock 🟨
**Purpose:** Prove the remaining combat and defensive-choice pillars before prototype polish.
**Must establish:** Readable melee goblin attacks, active defense through dash/block/parry, authored melee and ranged waves, two tower choices, two trap choices, and reliable broken-gate traversal.
**Completion definition:** *The prototype demonstrates skilled combat and meaningful defensive variety; its feature set is locked.*

- ⬜ #279 refactor(spawning): consolidate wave scheduling architecture
- ⬜ #267 feat(tower): add second prototype tower archetype
- ⬜ #264 feat(defense): add second prototype trap archetype
- ⬜ #112 refactor(player): split PlayerController responsibilities
- ⬜ #43 Implement enemy pass-through movement via broken gates
- ⬜ #31 Docs: Add short design doc for Spawning + Waves behavior

---

## P7 — Prototype Lock & Polish 🟨
**Purpose:** Lock the prototype content and make the experience readable, stable, and demonstrable.
**Must establish:** Presentation and feedback clarity, final input-contract regeneration, core tuning, bug fixes only, and no new mechanics.
**Completion definition:** *This is a complete Castlebound prototype.*

- ⬜ #287 feat(input): refine responsive floating mobile joysticks
- ⬜ #277 fix(ai): prevent enemies from stacking directly
- ⬜ #276 fix(player): prevent sticking against barrier and vault colliders
- ⬜ #275 fix(visual): split castle wall floor from foreground wall
- ⬜ #156 feat(visual): improve attack presentation readability at high swing speeds
- ⬜ #132 feat(visual): basic ground tileset
- ⬜ #120 Locked Palette + Palette-based Tints
- ⬜ #110 chore(input): regenerate PlayerControls.cs from inputactions
- ⬜ #105 feat(art): polish castle wall and tower sprite presentation
- ⬜ #104 fix(gameplay): bug fix sweep only (no new mechanics)
- ⬜ #103 chore(balance): core tuning pass
- ⬜ #30 UX: Add basic visual/audio feedback for melee hits and gate repairs

---

## Unassigned — Gameplay and Content Backlog

- ⬜ #284 feat(combat): add reusable enemy attack presentation profiles
- ⬜ #238 feat(loot): tune goblin drops by equipment and wave tier
- ⬜ #237 feat(projectiles): add knife and spear throwable variants
- ⬜ #230 feat(ui): show enemy knowledge gained after failure
- ⬜ #229 feat(ui): add progressive enemy bestiary
- ⬜ #228 feat(ui): unlock knowledge-driven enemy health bars
- ⬜ #227 feat(enemies): report sightings and confirmed kills
- ⬜ #226 feat(knowledge): persist enemy sightings and mastery
- ⬜ #157 feat(gameplay): add additional prototype weapons with distinct swing-speed tiers
- ⬜ #71 feat(loot): coin tier values (small/medium/large)
- ⬜ #70 feat(loot): pickup object pooling
- ⬜ #69 feat(loot): hybrid coin bursts (shower + stack remainder)
- ⬜ #59 feat(inventory): add item pull (magnet) near player

## Unassigned — Maintenance, Tooling, and Workflow Backlog

- ⬜ #144 refactor(ui): extract close button construction out of TouchUIBindings
- ⬜ #122 Docs — Living Checklists + Decisions Log
- ⬜ #119 Auto-wiring (Editor Tool)
- ⬜ #118 Prefab Creator (Editor Tool)
- ⬜ #117 Scene Validator (Editor Tool)
- ⬜ #113 refactor(ui): centralize upgrade button feedback handling
- ⬜ #111 perf(ui): pool upgrade menu rows
- ⬜ #36 Define Android versioning strategy for CI and test builds
- ⬜ #35 Android milestone release workflow (GitHub Releases)
- ⬜ #34 PR-gated Android APK artifact build (label-based)
- ⬜ #24 UI: Add “Feedback / Bug Report” menu option sending email

---

*To update this doc: run "sync issue tree" in Cowork, or manually edit after a planning session.*
