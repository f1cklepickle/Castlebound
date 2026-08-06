## 2026-08-06 - feat-enemy-parry-stagger

### Summary
- Added activation-snapshotted parry window and capacity with deterministic successful-parry consumption.
- Added an enemy-owned stagger authority for eligibility, duration, repeated-request rejection, recovery locking, and lifecycle cleanup.
- Added the explicit idempotent enemy attack cancellation boundary required for later melee integration.

### New or Updated Tests
**EditMode**
- PlayerDefenseStateMachineTests, PlayerDefenseControllerTests, and EnemyStaggerReceiverTests — activation snapshots, first-received capacity consumption, stagger timing, ignored repeats, recovery locking, normalization, and cleanup.

**PlayMode**
- N/A — N/A

### Notes
- PlayerDefenseStateMachineTests and PlayerDefenseControllerTests pass manually; EnemyStaggerReceiverTests awaits rerun after correcting its EditMode lifecycle invocation.

## 2026-08-05 - feat-player-defense-soft-anchor

### Summary
- Made the physical mobile defense button advance only when aim exceeds its current soft-anchor radius, retaining each new position until exceeded again or released.
- Clamped button drift to a maximum radius around its authored right-side screen position.
- Restored the button to its authored position on release, missing release, disable, pause, or focus loss.
- Tuned the authored soft-anchor radius to 160 pixels and maximum drift to 170 pixels.

### New or Updated Tests
**EditMode**
- TouchDefenseAimButtonTests — soft-follow threshold, radial maximum drift, continued aim direction, and snap-back lifecycle coverage.

**PlayMode**
- N/A — N/A

### Notes
- Full EditMode and PlayMode suites pass in the Unity Test Runner; simulator validation confirms block/parry feedback, reliable release, and bounded stateful soft-anchor behavior.

## 2026-08-04 - feat-player-frontal-block-parry

### Summary
- Added deterministic Idle, Parry Window, Blocking, and Recovery defense states with a 120-degree frontal melee resolver and Health-owned applied damage.
- Integrated attack cancellation, continuous guard aiming, 60% guarding movement, contextual enemy melee delivery, and projectile bypass behavior.
- Added right-mouse, left-trigger, and captured mobile defense/aim input plus a world-space guard arc for parry, block, success, and recovery feedback.
- Hardened mobile defense release with stable Input System touch identity and cleanup for missing release, cancellation, disable, pause, and focus loss.

### New or Updated Tests
**EditMode**
- PlayerDefenseStateMachineTests, PlayerDefenseHitResolverTests, PlayerDefenseControllerTests, PlayerGuardArcPresenterTests, PlayerDefensePrefabContractTests, TouchDefenseAimButtonTests, and DefenseInputContractsTests — state timing, inclusive arc boundaries, applied damage, attack cancellation, presentation, prefab wiring, stable touch release, stale-capture cleanup, and desktop/gamepad/mobile input contracts.

**PlayMode**
- PlayerDefenseEnemyAttackPlayTests — deterministic enemy-clock impact is negated by a frontal parry while preserving attacker identity.

### Notes
- Gameplay and test assemblies pass Roslyn compile validation; Unity EditMode and PlayMode execution remains pending manual Test Runner validation because the project is open in Unity.

## 2026-08-03 - refactor-269-normalized-attack-timing

### Summary
- Added one holder-neutral deterministic attack clock and authoritative attack-rate normalization policy with phase and swing overshoot carry.
- Migrated player and enemy attacks to immutable per-swing equipment snapshots, with player base damage authored as 1 and added to equipment damage.
- Removed the redundant player cooldown and rate calculators plus enemy coroutine waits, with focused runtime and resolver extractions to avoid controller growth.
- Bound enemy attack presentation to the deterministic clock through the Goblin Animator's explicit `AttackProgress` parameter, aligning every equipment rate with zero-based frame 6's authored downward strike at 0.3333 seconds.

### New or Updated Tests
**EditMode**
- AttackRatePolicyTests, AttackClockTests, EnemyAttackTimingTests, PlayerAttackBaseDamageTests, PlayerAttackSnapshotTests, PlayerAttackLoopTimingTests, EnemyAnimationPresenterTests, EnemyGoblinVisualContractTests, and player attack contract tests — normalization, base-plus-equipment damage, immutable timing, overshoot, exact-once impact, cancellation, snapshot delivery, explicit Animator progress, authored strike mapping, and ownership regressions.

**PlayMode**
- EnemyEquipmentCadencePlayTests and EnemyAttackPresentationSyncPlayTests — equipment changes cadence while unarmed and club rates both render zero-based frame 6 at the exact impact boundary.

### Notes
- Full EditMode and PlayMode suites passed in the Unity Test Runner; manual gameplay validation confirmed equipment-scaled cadence, base-plus-equipment damage, cancellation, and zero-based frame 6 enemy impacts.

## 2026-08-03 - refactor-equipment-entity-agnostic-profiles

### Summary
- Added wearer-neutral combat equipment profiles, capability validation, immutable resolved combat snapshots, and a shared runtime equipment-source contract.
- Separated shared combat effects from player inventory/presentation data and enemy spawning, role, grip, and target-layer data.
- Migrated player weapons and enemy unarmed, club, and rock definitions, with one shared club profile used by both player and enemy adapters.

### New or Updated Tests
**EditMode**
- CombatEquipmentProfileTests, CombatEquipmentAssetTests, ItemDefinitionTests, PlayerWeaponControllerTests, WeaponHandTests, EnemyEquipmentTests, and EnemyProjectileAttackDeliveryTests — capability checks, stat resolution, immutable snapshots, source events, shared-profile ownership, authored asset migration, and player/enemy regressions.

**PlayMode**
- GoblinEquipmentWavePlayTests — spawned enemy equipment retains the same shared profile referenced by a player weapon adapter.

### Notes
- EditMode and PlayMode suites pass per user validation; manual behavior validation confirmed existing equipment and combat behavior remain unchanged.

## 2026-08-03 - feat-spawning-enemy-equipment-distribution

### Summary
- Added reusable weighted enemy equipment loadout assets with deterministic seeded selection.
- Authored melee goblin club chance from 20% on wave 1 to 100% on wave 10 while keeping ranged goblins on rock equipment.
- Propagated selected equipment through spawn requests with compatible application and safe prefab-default fallback.

### New or Updated Tests
**EditMode**
- EnemyEquipmentLoadoutTableTests and GoblinEquipmentDistributionTests — weight progression, seeded replay, authored and generated loadout propagation, and archetype-count regression coverage.

**PlayMode**
- GoblinEquipmentWavePlayTests — compatible equipment application before first update and incompatible-equipment fallback.

### Notes
- EditMode and PlayMode suites pass per user validation; manual behavior validation confirmed the authored equipment distribution.

## 2026-08-02 - feat-spawning-goblin-wave-composition

### Summary
- Added canonical melee and ranged goblin archetype IDs with legacy grunt schedule compatibility.
- Authored exact opening-wave goblin counts and deterministic ranged and Lurker ramp progression.
- Standardized goblin prefab and loot-profile names while separating serialized wave data types into focused files.

### New or Updated Tests
**EditMode**
- GoblinWaveCompositionTests, EnemySpawnScheduleAssetBalanceTests, EnemySpawnerTests, WaveScheduleRampTests, WaveScheduleRuntimeTests, EnemyBalanceTableTests, and EnemyRangedPrefabContractTests — canonical IDs, exact counts, deterministic ordering, zero-count timing, ramp composition, balance aliases, mappings, and prefab regressions.

**PlayMode**
- EnemySpawnerRunnerPlayTests — mixed melee and ranged goblin prefab resolution from an authored wave.

### Notes
- EditMode and PlayMode suites pass per user validation; manual behavior validation confirmed the authored goblin composition and ramp progression.

## 2026-08-02 - feat-ai-ranged-engagement

### Summary
- Added maximum-only ranged engagement movement: ranged goblins approach from outside attack distance and remain stationary anywhere inside it.
- Added an optional hold-movement policy contract so melee enemies retain their existing surround orbit without duplicating range ownership.
- Assigned the stationary hold policy only to the ranged goblin prefab, preventing lateral orbit movement while preserving inward recovery at the range boundary.
- Reused the existing player-ring neighbor feed for speed-limited tangential ranged spacing while clearing stale separation when enemies leave player-targeting eligibility.

### New or Updated Tests
**EditMode**
- EnemyApproachSpreadTests, EnemyEngagementTests, EnemyLocomotionTests, EnemyRingEligibilityTests, and EnemyRangedPrefabContractTests — close-range eligibility, tangential hold spacing, stale-state cleanup, policy wiring, and melee orbit regression coverage.

**PlayMode**
- EnemyRangedAttackPlayTests — spawned ranged goblin stationary hold and neighbor separation behavior inside maximum attack distance.

### Notes
- EditMode and PlayMode suites pass per user validation, including the ranged spacing follow-up.

## 2026-08-01 - feat-enemies-ranged-goblin-rock-throw

### Summary
- Added an independently spawnable ranged goblin and a shared VisualRoot that keeps its animated rock hand aligned through every facing direction.
- Extracted shared projectile launching and interchangeable melee/projectile enemy attack delivery while preserving tower and melee behavior contracts.
- Replaced enum-based enemy loadouts with Unarmed, Club, and Rock data-driven equipment definitions.
- Restored the authored bottom-center goblin sprite pivots after isolating the directional drift to the visual hierarchy.
- Added inspector-tunable, frame-rate-independent visual spin to thrown rock projectiles.

### New or Updated Tests
**EditMode**
- ProjectileLauncherTests, ProjectileSpinTests, EnemyEquipmentTests, EnemyProjectileAttackDeliveryTests, EnemyRangedPrefabContractTests, EnemySurroundPrefabContractTests, EnemyAttackTests, EnemyGoblinVisualContractTests, and EnemyHitBarrierFeedbackTests — shared launching, visual spin, equipment compatibility and snapshots, delivery behavior, prefab contracts, facing hierarchy, and melee regressions.

**PlayMode**
- EnemyRangedAttackPlayTests and EnemyGoblinAnimationPlayTests — ranged rock launch smoke coverage and existing unarmed goblin regression coverage.

### Notes
- Tests not run per the user-owned validation boundary; compile-only batch validation was blocked because the project is open in another Unity instance.

## 2026-08-01 - melee-goblin-authored-animation

### Summary
- Replaced runtime goblin sprite-sheet slicing with Unity multi-sprite imports and authored Idle, Walk, and Attack clips.
- Added a real clip-animated HandSocket with explicit unarmed and club spawn-time equipment presentation.
- Preserved EnemyAttack as the authority for windup, impact, recovery, damage, and cooldown timing.

### New or Updated Tests
**EditMode**
- EnemyAnimationPresenterTests, EnemyGoblinVisualContractTests, and EnemyEquipmentTests — impact-anchored speed scaling, clip/socket/import contracts, and explicit unarmed equipment behavior.

**PlayMode**
- EnemyGoblinAnimationPlayTests — spawned goblin animation rig and default unarmed loadout smoke coverage.

### Notes
- EditMode and PlayMode suites pass per user validation.

## 2026-07-30 - event-driven-enemy-retargeting

### Summary
- Replaced spawn-owned home barriers with cached, event-driven barrier target decisions.
- Selected the nearest barrier from the affected enemy's current position only when retargeting is requested.
- Connected enemy and player region transitions to enemy-local target refresh requests.

### New or Updated Tests
**EditMode**
- EnemyTargetingTests — valid targets remain cached without polling and explicit retarget requests recompute the nearest barrier.

**PlayMode**
- EnemyTargetingPlayTests and BarrierRepairEnemyRetargetPlayTests — destroyed targets and repair expulsion trigger safe local retargeting.

### Notes
- Relevant targeting, steering, repair, engagement, and attack-lock regressions pass.

## 2026-07-30 - barrier-repair-enemy-retarget

### Summary
- Reconciled castle-region membership when barrier repair expels an overlapping enemy.
- Restored barrier targeting after enemies are pushed outside.
- Preserved inward overlap resolution and ordinary trigger-owned region tracking.

### New or Updated Tests
**EditMode**
- BarrierEnemyPushThresholdTests — outward overlap resolution reports that region reconciliation is required.

**PlayMode**
- BarrierRepairEnemyRetargetPlayTests — expelled enemies reacquire the repaired barrier whether or not they had entered the castle trigger.

### Notes
- Relevant barrier, enemy targeting, engagement, and attack-lock regressions pass.

## 2026-07-30 - melee-goblin-animation

### Summary
- Added prefab-authored melee goblin presentation with attack-priority transitions and authored attack-frame cadence.
- Kept enemy windup, damage, and cooldown timing authoritative in EnemyAttack.
- Preserved top-down directional rotation through the existing EnemyFacing visual contract.

### New or Updated Tests
**EditMode**
- EnemyAnimationPresenterTests — attack priority, movement-driven walking, delayed idle, impact hold, and authored recovery timing.
- EnemyGoblinVisualContractTests — sprite import settings, sheet dimensions, frame counts, and prefab references.

**PlayMode**
- EnemyEngagementPlayTests, EnemyAttackTargetLockPlayTests, and EnemyTargetingPlayTests — existing enemy combat and targeting regression coverage.

### Notes
- Full EditMode and PlayMode suites pass after final animation timing and prefab serialization validation.
