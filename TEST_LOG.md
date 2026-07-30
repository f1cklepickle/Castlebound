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
