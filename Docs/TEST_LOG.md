# Castlebound - Test Log

> Purpose: Track what tests exist, what changed, failures, known flakiness, and next steps.

---

## 2026-02-01 - feat/castle-pressure-trigger

### Summary
- Added per-barrier break pressure trigger core with wave-aware reset.
- Wired barrier break events to pressure tracking and scene wave index provider.
- Added EditMode tests for trigger rules and barrier break event regression.

### New or Updated Tests
**EditMode**
- `BarrierBreakPressureTriggerTests` — third-break trigger and wave reset behavior
- `BarrierPressureTrackerTests` — per-barrier trigger and wave change reset
- `BarrierHealthTests` — OnBroken event fires once per break

**PlayMode**
- N/A — N/A

### Notes
- Manual: verified trigger fires on 3rd break and resets next wave using temporary debug listener.

## 2026-01-26 - feat/barrier-tier-visuals

### Summary
- Added barrier tier color cycle logic with star carry and capped end state.
- Added world-space barrier healthbar prefab with border/star visuals and health fill.
- Added binder to sync tier + health into visuals.
- Added camera follow auto-assign fallback and HUD inventory fallbacks for Player swaps.

### New or Updated Tests
**EditMode**
- `BarrierTierColorCycleTests`
- `BarrierTierVisualsTests`
- `BarrierTierVisualsBinderTests`
- `CameraFollowTests`
- `HudInventoryFallbackTests`

### Notes
- Manual: upgrade barriers in `MainPrototype` to verify shade progression and star gating.
- Manual: verify camera follows Player and HUD updates after swapping Player instance.

## 2026-01-21 - feat/barrier-upgrade-action

### Summary
- Added pre-wave upgrade menu with 5s gap auto-open and manual U toggle.
- Added upgrade list UI that auto-discovers upgradeable barriers, updates after upgrades, and shows hover/success/denied feedback.
- Upgrades add current health delta and revive broken barriers once health crosses above 0.
- Added pre-wave gating and player input pause while the menu is open.

### New or Updated Tests
**EditMode**
- `BarrierUpgradeActionTests`
- `UpgradeMenuControllerTests`
- `WaveMenuGateTests`

### Notes
- Upgrade menu opens after the wave gap completes, blocks in-wave toggles, and starts the next wave on close.

## 2026-01-18 - feat/barrier-upgrade-persist

### Summary
- Added barrier upgrade persistence across wave transitions for shared and local states.
- Added wave-start event hook to reapply tiered max health without resetting current health.

### New or Updated Tests
**EditMode**
- `BarrierUpgradePersistenceTests`

### Notes
- Shared state assets allow linked upgrades; unassigned state uses per-barrier local tier.

## 2026-01-18 - feat/barrier-upgrades

### Summary
- Added barrier upgrade config, tier state, and purchase flow with gold spend validation.
- Added linear scaling helpers for max health and upgrade cost per tier.

### New or Updated Tests
**EditMode**
- `BarrierUpgradeTests`

### Notes
- Upgrade purchase succeeds only when gold covers the current tier cost.

## 2026-01-15 - feat/loot-tables

### Summary
- Added configurable loot tables with per-table rolls, global cap, and rarity-first selection.
- Loot drops now split gold into multiple pickups with per-table spawn caps and spill motion away from player.
- Added pickup delay and restricted pickups to the player body collider.

### New or Updated Tests
**EditMode**
- `LootTableRngTests`
- `LootDropperTests`
- `ItemPickupComponentTests`
- `LootSpillMotionTests`

### Notes
- LootDropper uses table-to-prefab mapping with max rolls and per-table spawn caps.
- Spill motion uses cone width + distance range tuning; pickups slide with ease-out.

## 2026-01-10 - feat/weapon-hand-sprite

### Summary
- Added weapon hand socket rendering from weapon definitions.
- Added weapon hitbox sizing and offset from weapon definitions.

### New or Updated Tests
**EditMode**
- `WeaponHandTests`
- `HitboxSizeTests`

### Notes
- Manual: In MainPrototype, equip sword; verify hand sprite follows attack socket and hitbox size/offset matches weapon definition during swings.

## 2026-01-10 - feat/weapon-slot-switch

### Summary
- Added scroll-based weapon slot swapping with a short cooldown.
- Empty slot now represents unarmed state.

### New or Updated Tests
**EditMode**
- `InventoryStateTests`
- `PlayerWeaponControllerTests`
- `PlayerWeaponSlotSwapInputTests`

### Notes
- Manual: In MainPrototype, scroll to swap slots; verify HUD highlight and damage updates when switching to empty slot.

## 2026-01-09 - feat/loot-assets-gold-weapon

### Summary
- Added gold + sword item definitions and pickup prefabs; organized item/pickup folders.
- Wired weapon resolver + damage to reflect weapon definitions.

### New or Updated Tests
**EditMode**
- `ItemPickupTests` (event emissions for gold/weapon pickups)

### Notes
- Manual: In MainPrototype, pick up sword and confirm HUD icon + increased damage; pick up gold and confirm gold count.

## 2026-01-08 - feat/hud-weapon-gold-xp

### Summary
- Added HUD bindings for weapon slots and gold/xp, plus scene wiring in MainPrototype.

### New or Updated Tests
**EditMode**
- `GoldXpHudTests`
- `WeaponSlotsHudTests`

### Notes
- Manual: In MainPrototype, verify gold/xp text updates and weapon slot highlight swaps when active slot changes.

## 2026-01-06 - feat/potion-use

### Summary
- Added potion consume + cooldown + HUD slot, plus pickup prefab and wiring.

### New or Updated Tests
**EditMode**
- `PotionConsumeTests`
- `PotionUseControllerTests`

### Notes
- Manual: In MainPrototype, pick up potion, verify HUD icon/count updates, use potion to decrement stack, cooldown overlay drains and blocks repeat use until elapsed.

## 2026-01-05 - feat/weapon-equip-attack

### Summary
- Added PlayerWeaponController and weapon stat mapping from inventory definitions.

### New or Updated Tests
**EditMode**
- `PlayerWeaponControllerTests`

### Notes
- Weapon stats are exposed as a data object for attack integration.

## 2026-01-04 - feat/item-pickups

### Summary
- Added pickup pipeline logic for weapons, potions, gold, and xp.

### New or Updated Tests
**EditMode**
- `ItemPickupTests`
- `ItemPickupComponentTests`

### Notes
- Auto pickup is allowed only when inventory can accept the item.
- Manual pickup swaps the active weapon when slots are full.

## 2026-01-04 - feat/inventory-item-definitions

### Summary
- Added ItemDefinition, WeaponDefinition, and PotionDefinition ScriptableObjects for inventory items.

### New or Updated Tests
**EditMode**
- `ItemDefinitionTests`

### Notes
- Weapon fields: damage, attack speed, hitbox size, knockback.
- Potion fields: heal amount, cooldown seconds.

## 2026-01-04 - feat/inventory-runtime-state

### Summary
- Added runtime-only InventoryState for 2 weapon slots, potion stack, and gold/xp counters.

### New or Updated Tests
**EditMode**
- `InventoryStateTests`

### Notes
- Inventory uses ID-based items for MVP; assets will follow in the next issue.

## 2026-01-02 - fix/barrier-repair-overlap

### Summary
- Resolve player/enemy overlap when barriers are repaired or re-enabled.
- Player is pushed inward relative to the barrier anchor; enemies push in/out based on overlap.

### New or Updated Tests
**EditMode**
- `BarrierEnemyPushThresholdTests`
- `BarrierOverlapResolverTests`
- `BarrierRepairOverlapPushTests`
- `BarrierRepairOverlapAnchorTests`
- `BarrierOverlapResolverPushInTests`

**PlayMode**
- `BarrierRepairOverlapIntegrationPlayTests`

### Notes
- Player placement uses barrier anchor direction to ensure inward push.
- Overlap resolution runs on repair/enable; EditMode and PlayMode tests pass.

## 2025-12-27 - feat/combat-feedback

### Summary
- Added combat feedback cues for player hit, player hit enemy, and enemy hit barrier.
- Wired visual feedback: player edge flash, enemy red flash, barrier jitter.

### New or Updated Tests
**EditMode**
- `FeedbackEventChannelContractTests`
- `PlayerHitFeedbackTests`
- `PlayerHitEnemyFeedbackTests`
- `EnemyHitBarrierFeedbackTests`

### Notes
- Feedback cues use a shared event channel; listeners can be stacked.
- Enemy hit feedback uses target instance ID for precise flashing.

## 2025-12-19 - feat/spawner-waves-ramp

### Summary
- Added wave/ramp-based spawner runtime (per-wave strategy, wait-for-clear + gap, maxAlive, shuffle fairness).
- Added gate auto-ID (GateIdProvider + SpawnPointMarker fallback) and runner auto-find of markers.
- Added alive-count tracking via lifetime callback and wired MainPrototype gates/runner to the new schedule (authored wave + ramp).

### New or Updated Tests
**EditMode**
- `SpawnMarkerOrderBuilderTests` (fair shuffle coverage/determinism; warn when spawnCount < markers)
- `WaveScheduleRuntimeTests` (per-wave override/defaults)
- `WaveScheduleRampTests` (count ramp, tier unlocks, weighted selection)
- `SpawnedEntityLifetimeTests` (callback invoked once on despawn)

**PlayMode**
- `EnemySpawnerRunnerPlayTests`
  - `SpawnsEnemiesAtMarkersOverTime` (updated to top-level SpawnSequenceConfig)

### Notes
- Ramp defaults: base 6, +2 every 2 waves, start wave 1; strategy RoundRobin; wait-for-clear true; gap 5s; maxAlive 0.
- MainPrototype gates include spawn markers; runner uses auto-find and prefab mapping for grunt.

## 2025-12-10 - fix/barrier-targeting-anchors

### Summary
- Stabilized enemy home barrier assignment: uses barrier approach anchors when present, breaks distance ties deterministically, and assigns in Start after barriers register.
- Prevented double damage on multi-collider targets by deduping IDamageable hits per attack.
- Wired barrier anchors/gizmos and placed gates on all walls in MainPrototype for consistent spawning/attacks.

### New or Updated Tests
**EditMode**
- `EnemyBarrierApproachAnchorTests`
  - `EntersHold_WhenApproachingFromSide_UsesAnchorPosition`

**Play/Manual**
- Manual/PlayMode: verify enemies at N/E/S/W spawn positions pick the nearest gate and damage barriers by 1 per attack.

### Notes
- Enemy hold reseat bias increased for normal approach speed.
- Home assignment now runs after barriers register to avoid partial lists.

## 2025-12-10 - feat/enemy-spawner-basic

### Summary
- Added data-driven enemy spawner (round-robin gate coverage, timed sequences) and runtime bootstrap (schedule asset + spawn markers + runner).
- Ensured end-to-end spawning works via PlayMode smoke: schedule asset + markers + prefab mapping instantiate enemies at marker positions over time.

### New or Updated Tests
**EditMode**
- `EnemySpawnerTests`
  - `SelectsEachGateBeforeRepeating`
  - `RespectsInitialDelayAndIntervals`

**PlayMode**
- `EnemySpawnerRunnerPlayTests`
  - `SpawnsEnemiesAtMarkersOverTime`

### Notes
- Runner builds spawn points from `SpawnPointMarker` components, reads `EnemySpawnScheduleAsset`, and uses enemyTypeId prefab mappings to instantiate.
- Round-robin selection guarantees every gate gets spawns before repeating.
- Future work: barrier approach anchors for reliable barrier contact; optional PlayMode smokes for that flow.

## 2025-12-16 - fix/barrier-inside-attack-gate

### Summary
- Block barrier damage when enemy is inside and player is inside.
- Barrier attacks gate on CastleRegionTracker state and detect barriers via parent to avoid hitbox bypass.

### New or Updated Tests
**EditMode**
- `EnemyBarrierAttackGateTests`
  - `AllowsBarrierDamage_WhenEnemyOutside`
  - `BlocksBarrierDamage_WhenEnemyInside_PlayerInside`

### Notes
- EnemyAttack caches region lookup and dedupes barrier damage even with hitbox colliders.
- Targeting unchanged; inside enemies still prioritize player when player is outside.
- Known issue: enemies still damage barriers from inside; follow-up fix needed.

## 2025-12-07 - feat/enemy-home-barrier-targeting

### Summary
- Enemies now assign a home barrier at spawn (nearest regardless of health) and steer to it while outside; they hand off to the player after passing the broken barrier opening.
- Steering was decoupled from ring spacing so orbit behavior stays anchored on the player; ring manager now centers on the player, not transient targets.

### New or Updated Tests
**EditMode**
- `CastleTargetSelectorHomeBarrierTests`
  - `AssignsHomeBarrier_BasedOnSpawnPosition`
  - `KeepsHomeBarrier_EvenIfAnotherBecomesCloser`
  - `UsesAssignedBarrier_WhenPlayerInside_EnemyOutside_EvenIfBroken`
  - `TargetsPlayerAfterHomeBarrierBroken`

- `EnemyControllerHomeBarrierTargetingTests`
  - `RetainsHomeBarrierOutside_WhenBroken_ThenTargetsPlayerInside`

- `EnemyControllerSteeringStateTests`
  - `SteersToHomeBarrier_Outside_AndPlayer_Inside`

- `CastleRegionTrackerTests`
  - `RegistersPlayerAndEnemy_OnTriggerEnter_AndClears_OnExit`

### Notes
- Controller retains orbit/tangent spacing because ring manager now anchors on player.
- Steering handoff uses proximity to a broken home barrier to switch to player and avoid jitter at the opening.

## 2025-12-04 - feat/gate-destruction-repair

### Summary
- Destructible barrier + repair system implemented and merged.
- Enemy targeting handles player inside/outside, barrier intact vs broken, and repair retargeting.

### New or Updated Tests
**EditMode**
- `BarrierHealthTests`
  - `Breaks_WhenHealthReachesZero`
  - `DisablesColliderAndSprite_WhenBroken`
  - `Repairs_WhenRepaired_FromBrokenState`
  - `Restores_ColliderAndSprite_WhenRepaired`

- `EnemyAttackTests`
  - `EnemyAttack_DealsDamage_ToIDamageableTarget`
  - `EnemyAttack_BreaksBarrier_WhenDamageExceedsHealth`
  - `EnemyAttack_DoesNotDamage_WhenBarrierAlreadyBroken`

- `CastleTargetSelectorTests`
  - `ReturnsPlayer_WhenPlayerOutside_RegardlessOfGates`
  - `ReturnsGate_WhenPlayerInside_EnemyOutside_GatePresent`
  - `ReturnsPlayer_WhenBarrierBroken`
  - `ReturnsGate_AfterGateReappears_WhilePlayerInside_EnemyOutside`
  - `ReturnsPlayer_WhenPlayerAndEnemyInside`

- `EnemyBarrierHoldBehaviorTests`
  - `CanHold_WhenBarrierIntact_AndWithinHoldRadius`
  - `DoesNotHold_WhenBarrierIsBroken`

### Notes
- Barriers no longer use Health.cs (that remains for player/enemy only).
- All enemy structure damage routes through IDamageable.
- R-key repair interaction added as part of the manual playtesting loop.

## 2025-12-01 - selector broken-barrier pivot

### Summary
- CastleTargetSelector treats a broken barrier as absent and returns the player instead of the gate.
- Updated selector tests to expect the player when the barrier is broken.

### New or Updated Tests
**EditMode**
- `CastleTargetSelectorTests` (broken barrier behavior)

### Notes
- No behavior change outside targeting pivot.

## 2025-11-16 - feat/enemy-barrier-targeting

### Summary
- Replaced the linear barrier targeting helper with castle-region-based gate targeting.
- Enemies choose between player and gates using inside/outside flags and nearest gate logic.

### New or Updated Tests
**EditMode**
- `CastleTargetSelectorTests`
  - `SelectsPlayer_IfBothOutside`
  - `SelectsGate_WhenPlayerInside_EnemyOutside`
  - `SelectsPlayer_IfEnemyAndPlayerInside`
  - `SelectsPlayer_IfEnemyInside_PlayerOutside`
  - `SelectsNearestGate_WhenMultipleExist`
  - `DefaultsToPlayer_WhenNoGates`

- `EnemyControllerBarrierTargetingTests`
  - `UsesSelector_ForGateVsPlayerSelection`

### Notes
- CastleRegionTracker defines castle interior.
- Gate Transform acts as an entrance waypoint; enemies switch to player once inside.
- Future work: gate destruction + repair UI with waypoint persistence.

## 2025-09-25 - init test log

### Summary
- Added TEST_LOG.md to track test intent and outcomes.

### New or Updated Tests
- None.

### Notes
- Unity Test Framework installed (com.unity.test-framework).
- Assemblies present: `_Project.Tests.EditMode`, `_Project.Tests.PlayMode`.
- Initial status: no test files yet.
- Planned next: `HealthTests.cs`, `TargetSelectorTests.cs`.
