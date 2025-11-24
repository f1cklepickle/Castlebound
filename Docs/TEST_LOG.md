# Castlebound — Test Log

> Purpose: Track what tests exist, what changed, failures, known flakiness, and next steps.

---

## 2025-09-25 — Init test log
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
