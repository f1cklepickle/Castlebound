# Castlebound — Living Issue Tree
**Last synced:** 2026-08-24 | **Repo:** https://github.com/f1cklepickle/Castlebound

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
| P6 — Prototype Completion | Finish the gameplay, content, controls, behavior, and presentation required for a complete prototype | 🟨 In Progress (13/26, 50%) |
| P7 — Prototype Polish & Release Candidate | Polish, tune, synchronize, stabilize, and validate the completed prototype | ⏸ Queued until P6 closes (18/21, 86%) |

> Closed issues retain their historical milestone assignments; the reorganized P6/P7 boundary applies to remaining open work.

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

## P6 — Prototype Completion 🟨
**Purpose:** Finish all required gameplay, content, controls, behavior, and presentation needed to call the prototype complete.
**Completion definition:** *The intended prototype experience is functionally and presentationally complete, and its feature/content scope can be frozen.*

**Foundation, behavior, and controls**
- ⬜ #279 refactor(spawning): consolidate wave scheduling architecture
- ⬜ #43 Implement enemy pass-through movement via broken gates
- ⬜ #296 fix(ai): smooth enemy chase-to-surround transition
- ⬜ #287 feat(input): refine responsive floating mobile joysticks

**Defense choices and visual dependencies**
- ⬜ #120 Locked Palette + Palette-based Tints
- ⬜ #275 fix(visual): split castle wall floor from foreground wall
- ⬜ #267 feat(tower): add second prototype tower archetype
- ⬜ #264 feat(defense): add second prototype trap archetype

**Presentation and close-out**
- ⬜ #105 feat(art): polish castle wall and tower sprite presentation
- ⬜ #132 feat(visual): basic ground tileset
- ⬜ #156 feat(visual): improve attack presentation readability at high swing speeds
- ⬜ #30 feat(audio): add melee-hit and gate-repair feedback cues
- ⬜ #31 Docs: Add short design doc for Spawning + Waves behavior

**P6 completion gate**
- [ ] All required implementation and content work is resolved
- [ ] Integrated prototype-completion validation is performed
- [ ] Feature and content scope is frozen
- [ ] P6 is closed before P7 becomes the primary active milestone

---

## P7 — Prototype Polish & Release Candidate ⏸
**Status:** Queued until P6 closes.
**Purpose:** Polish, tune, synchronize, stabilize, and validate the completed prototype as a release candidate.
**Completion definition:** *Castlebound has a stable, balanced, validated prototype release candidate.*

1. ⬜ #103 chore(balance): final core balance and pacing gate
2. ⬜ #110 chore(input): final generated-input synchronization
3. ⬜ #104 fix(gameplay): final regression and stabilization gate
4. ⬜ Release-candidate validation gate

**P7 completion gate**
- [ ] Final balance and pacing are complete
- [ ] Generated input is synchronized from the frozen authoritative asset
- [ ] Final regression and bug stabilization are complete
- [ ] The prototype release candidate is validated

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

- ⬜ #112 refactor(player): split PlayerController responsibilities
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
