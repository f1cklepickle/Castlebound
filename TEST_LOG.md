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
