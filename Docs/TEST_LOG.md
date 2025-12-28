# Castlebound — Test Log

> Purpose: Track what tests exist, what changed, failures, known flakiness, and next steps.

---

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

## 2025-09-25 - Init test log
**Scope:** Foundations  
**Added:** TEST_LOG.md; no test files yet (EditMode/PlayMode assemblies exist)  
**Why:** Establish a single place to summarize test intent and outcomes for future PRs  
**Status:** ✅

**Planned next:**
- Add EditMode: `HealthTests.cs`
- Add EditMode: `TargetSelectorTests.cs`

**Notes:**
- Unity Test Framework already installed (`com.unity.test-framework`)  
- Assemblies present: `_Project.Tests.EditMode`, `_Project.Tests.PlayMode`  

## 2025-11-16 - feat/enemy-barrier-targeting

### Summary
- Replaced the linear "barrier on line" targeting helper with a castle-region-based gate targeting system.
- Enemies now choose between the player and gate waypoints using inside/outside flags and nearest gate logic.

### New tests
- `CastleTargetSelectorTests`
  - `SelectsPlayer_IfBothOutside`
  - `SelectsGate_WhenPlayerInside_EnemyOutside`
  - `SelectsPlayer_IfEnemyAndPlayerInside`
  - `SelectsPlayer_IfEnemyInside_PlayerOutside`
  - `SelectsNearestGate_WhenMultipleExist`
  - `DefaultsToPlayer_WhenNoGates`
- `EnemyControllerBarrierTargetingTests`
  - Verifies `EnemyController2D` uses the selector to pick gate vs player correctly based on castle region flags.

### Removed tests
- Old line-based barrier targeting tests (`EnemyBarrierTargeting`), which encoded the previous spec:
  - "Barrier blocks direct line between enemy and player."
- These were removed because the design pivoted to an inside/outside castle + gate waypoint behavior, and the old spec no longer matches gameplay goals.

### Notes
- `CastleRegionTracker` with a `PolygonCollider2D` defines the castle interior.
- Gate `Transform` acts as an entrance waypoint; enemies switch to chasing the player once inside the region.
- Future work: gate destruction + repair UI, with gate collider/visual toggling while keeping the Transform as an AI waypoint.

## 2025-12-01 — selector broken-barrier pivot

- `CastleTargetSelector` now treats a broken barrier as absent and returns the player instead of the gate.
- Updated selector tests to expect the player when the barrier is broken.

## 2025-11-24 — feat/gate-destruction-repair (WIP)

### Summary
Begin implementing destructible castle barriers and unifying enemy attack logic via the `IDamageable` interface.

### New tests
- **BarrierHealthTests**
  - `Breaks_WhenHealthReachesZero`
  - `DisablesColliderAndSprite_WhenBroken`
- **EnemyAttackTests**
  - `EnemyAttack_DealsDamage_ToIDamageableTarget`

### Behavior added
- `BarrierHealth` now:
  - Implements `IDamageable`
  - Disables its collider and sprite on break
  - Uses lazy component caching for EditMode stability
- `EnemyAttack` now:
  - Exposes a public `Damage` value
  - Implements `DealDamage(IDamageable target)` for unified damage application

### Notes
- Barriers no longer use `Health.cs` (that remains for player/enemy only).
- All enemy → structure damage will be routed through `IDamageable` to support:
  - Player damage
  - Barrier damage
  - Future destructible objects
- Scene wiring and collision→IDamageable refactor planned next.

### Next planned tests
- Enemy → barrier contact triggers damage through `IDamageable`
- Barrier repair flow:
  - `Repairs_WhenHealedAboveZero`
  - `Reenables_ColliderAndSprite_WhenRepaired`

---

## 2025-12-04 — feat/gate-destruction-repair (COMPLETED)

### Summary
The destructible barrier + repair system has been fully implemented and merged. 
Enemy targeting now correctly handles:
- Player inside/outside castle region
- Barrier intact vs broken
- Rebuild retargeting (enemy switches back to barrier when repaired)
All EditMode tests pass.

### New or Updated Tests
**BarrierHealthTests**
- `Breaks_WhenHealthReachesZero`
- `DisablesColliderAndSprite_WhenBroken`
- `Repairs_WhenRepaired_FromBrokenState`
- `Restores_ColliderAndSprite_WhenRepaired`

**EnemyAttackTests**
- `EnemyAttack_DealsDamage_ToIDamageableTarget`
- `EnemyAttack_BreaksBarrier_WhenDamageExceedsHealth`
- `EnemyAttack_DoesNotDamage_WhenBarrierAlreadyBroken`

**CastleTargetSelectorTests**
- `ReturnsPlayer_WhenPlayerOutside_RegardlessOfGates`
- `ReturnsGate_WhenPlayerInside_EnemyOutside_GatePresent`
- `ReturnsPlayer_WhenBarrierBroken`
- `ReturnsGate_AfterGateReappears_WhilePlayerInside_EnemyOutside`
- `ReturnsPlayer_WhenPlayerAndEnemyInside`

**EnemyBarrierHoldBehaviorTests**
- `CanHold_WhenBarrierIntact_AndWithinHoldRadius`
- `DoesNotHold_WhenBarrierIsBroken`

### Notes
- This entry closes out the previously “WIP” 2025-11-24 section.
- R-key repair interaction added as part of the manual playtesting loop.

### Future Work
- Multi-barrier targeting & nearest-barrier selection (test name: `Enemy_UsesNearestBarrier_FromRegisteredBarriers`) will be implemented in a future feature branch (e.g. `feat/multi-barrier-targeting`).

## 2025-12-07 - feat/enemy-home-barrier-targeting

### Summary
- Enemies now assign a home barrier at spawn (nearest regardless of health) and steer to it while outside; they hand off to the player after passing the broken barrier opening.
- Steering was decoupled from ring spacing so orbit behavior stays anchored on the player; ring manager now centers on the player, not transient targets.

### New or Updated Tests
**CastleTargetSelectorHomeBarrierTests**
- `AssignsHomeBarrier_BasedOnSpawnPosition`
- `KeepsHomeBarrier_EvenIfAnotherBecomesCloser`
- `UsesAssignedBarrier_WhenPlayerInside_EnemyOutside_EvenIfBroken`
- `TargetsPlayerAfterHomeBarrierBroken`

**EnemyControllerHomeBarrierTargetingTests**
- `RetainsHomeBarrierOutside_WhenBroken_ThenTargetsPlayerInside`

### Notes
- Controller retains orbit/tangent spacing because ring manager now anchors on player.
- Steering handoff uses proximity to a broken home barrier to switch to player and avoid jitter at the opening.

### Additional tests (2025-12-07)
- `EnemyControllerSteeringStateTests`
  - `SteersToHomeBarrier_Outside_AndPlayer_Inside`
- `CastleRegionTrackerTests`
  - `RegistersPlayerAndEnemy_OnTriggerEnter_AndClears_OnExit`

## 2025-12-10 - fix/barrier-targeting-anchors

### Summary
- Stabilized enemy home barrier assignment: uses barrier approach anchors when present, breaks distance ties deterministically, and assigns in Start after barriers register.
- Prevented double damage on multi-collider targets by deduping `IDamageable` hits per attack.
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

## 2025-12-18 - refactor/enemy-targeting-movement-attack

### Summary
- Introduced EnemyTargetSelector with broken-home pass-through and target type; EnemyController2D consumes selector outputs.
- EnemyAttack gates by target type, defaults barrier gating to outside if CastleRegionTracker is missing (warning logged).
- Movement calculation extracted to EnemyMovement helper; orbit vs barrier tangents tested.
- Cached player in EnemyRingManager to avoid per-frame finds.
- Added validation warnings (via Debug_ValidateRefs) for missing player/home barrier; player tag fallback test.
- Target mask defaults: EnemyAttack sets Player layer when unset; preserves when set.
- Removed unused controller pass-through fields.

### New or Updated Tests
**EditMode**
- `EnemyTargetSelectorTests` (barrier vs player, broken home pass-through)
- `EnemyAttackTargetTypeTests` (gating by target type)
- `EnemyAttackRegionTrackerFallbackTests` (missing tracker warning/default outside)
- `EnemyAttackTargetMaskTests` (mask default/preserve)
- `EnemyControllerValidationTests` (Player tag fallback)
- `EnemyControllerOrbitVsBarrierTests` (tangent only for player target)
- `EnemyControllerWarningsTests` (warnings via Debug_ValidateRefs for missing player/home barrier)

### Notes
- Barrier damage gating aligned with behavior: inside+playerInside blocked; inside+playerOutside not exercised due to targeting.
- Missing CastleRegionTracker logs a warning and treats enemy/player as outside for barrier gating.
- Ring manager now caches player; no behavior change expected.

## 2025-12-XX - fix/barrier-inside-attack-gate

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
- Runner builds spawn points from `SpawnPointMarker` components, reads `EnemySpawnScheduleAsset`, and uses `enemyTypeId` → prefab mappings to instantiate.
- Round-robin selection guarantees every gate gets spawns before repeating.
- Future work: barrier approach anchors for reliable barrier contact; optional PlayMode smokes for that flow.
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
- MainPrototype gates include spawn markers; runner uses auto-find and prefab mapping for `grunt`.
