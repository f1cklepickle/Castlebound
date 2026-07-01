# Castlebound — Living Issue Tree
**Last synced:** 2026-06-30 | **Repo:** https://github.com/f1cklepickle/Castlebound

> This doc mirrors GitHub milestone/issue state as a readable progress snapshot.
> It is not the source of truth — GitHub is. Update this after planning sessions or milestone closes.
> To refresh: type "sync issue tree" in Cowork.

---

## Milestone Summary

| Milestone | Purpose | Status |
|-----------|---------|--------|
| P1 — Living Structure: Endless Power | Prove one structure can grow endlessly and visibly | ✅ Complete (8/8) |
| P2 — Castle Responds to Care | Prove the castle subtly assists defense | ✅ Complete (4/4) |
| P3 — Authored Defense Under Pressure | Prove the player is authoring defense, not reacting randomly | 🔄 In Progress (47/48 — 98%) |
| P4 — Castle Memory & Trust | Prove the castle remembers what the player brings back | ⬜ Not Started (0/9) |
| P5 — Meaningful Failure | Teach Castlebound's philosophy through loss | ⬜ Not Started (0/5) |
| P6 — Prototype Lock & Polish | Stop adding systems and make the experience readable and stable | ⬜ Not Started (0/23) |

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

## P3 — Authored Defense Under Pressure 🔄 98%
**Purpose:** Prove the player is authoring the defense, not reacting randomly.
**Must establish:** One deliberate pre-wave choice, no mid-wave construction, choice meaningfully affects defense outcome, clear cause → effect across waves.

- ✅ 47 issues closed
- ⬜ #132 feat(visual): basic ground tileset

---

## P4 — Castle Memory & Trust ⬜
**Purpose:** Prove the castle remembers what the player brings back and never surprises them.
**Must establish:** Loadout / Backpack / Castle Inventory separation, backpack clears at wave end, items persist safely in castle inventory, no auto-destruction, clear combat ↔ stewardship boundary.

- ⬜ #95 feat(inventory): wave-end transfer rules (backpack → vault)
- ⬜ #94 feat(ui): inventory panel tabs (Backpack / Castle Vault)
- ⬜ #93 feat(inventory): equip from Backpack mid-wave (via HUD slots)
- ⬜ #92 feat(inventory): dump Backpack → Castle Inventory at wave end
- ⬜ #91 feat(inventory): add Backpack (mid-wave carry)
- ⬜ #90 feat(inventory): add Castle Inventory (persistent vault)
- ⬜ #89 feat(shop): purchase integration (potions/weapons to backpack/vault)
- ⬜ #85 feat(shop): between-wave shop panel (potions/weapons)
- ⬜ #27 Spawning: Support multiple enemy types in one EnemySpawnSchedule

---

## P5 — Meaningful Failure ⬜
**Purpose:** Teach the philosophy of Castlebound through loss.
**Must establish:** Failure screen, waves survived displayed, one non-blaming philosophical line, fast restart flow.

- ⬜ #100 feat(stats): run stats tracking service
- ⬜ #99 feat(ui): run summary stats (kills, damage, repairs, gold)
- ⬜ #98 feat(flow): fast restart flow
- ⬜ #97 feat(ui): philosophical failure line
- ⬜ #96 feat(ui): failure screen with waves survived

---

## P6 — Prototype Lock & Polish ⬜
**Purpose:** Stop adding systems and make the experience readable and stable.
**Must establish:** Feedback noise reduction, camera clarity, core tuning pass, bug fixes only, no new mechanics.
**Completion definition:** *"This is a complete Castlebound prototype."*

- ⬜ #157 feat(gameplay): add additional prototype weapons with distinct swing-speed tiers
- ⬜ #156 feat(visual): improve attack presentation readability at high swing speeds
- ⬜ #144 refactor(ui): extract close button construction out of TouchUIBindings
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
- ⬜ #60 feat(camera): pixel-perfect setup + 2D best-practice defaults
- ⬜ #45 Add feedback cooldown/aggregation to reduce hit flash spam
- ⬜ #44 Remove unused releaseMargin from barrier hold behavior
- ⬜ #43 Implement enemy pass-through movement via broken gates
- ⬜ #42 Fix barrier damage gating when player and enemy are inside
- ⬜ #31 Docs: Add short design doc for Spawning + Waves behavior
- ⬜ #30 UX: Add basic visual/audio feedback for melee hits and gate repairs

---

## Unassigned
- ⬜ #122 Docs — Living Checklists + Decisions Log *(no milestone assigned)*
- ⬜ #71 feat(loot): coin tier values (small/medium/large) *(no milestone assigned)*
- ⬜ #70 feat(loot): pickup object pooling *(no milestone assigned)*
- ⬜ #69 feat(loot): hybrid coin bursts (shower + stack remainder) *(no milestone assigned)*
- ⬜ #59 feat(inventory): add item pull (magnet) near player *(no milestone assigned)*
- ⬜ #36 Define Android versioning strategy for CI and test builds *(no milestone assigned)*
- ⬜ #35 Android milestone release workflow (GitHub Releases) *(no milestone assigned)*
- ⬜ #34 PR-gated Android APK artifact build (label-based) *(no milestone assigned)*
- ⬜ #24 UI: Add “Feedback / Bug Report” menu option sending email *(no milestone assigned)*

---

*To update this doc: run "sync issue tree" in Cowork, or manually edit after a planning session.*
