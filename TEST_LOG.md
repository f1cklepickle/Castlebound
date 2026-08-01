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
