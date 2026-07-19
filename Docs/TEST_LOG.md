# Castlebound - Test Log

> Purpose: Track what tests exist, what changed, failures, known flakiness, and next steps.

---

## 2026-07-18 - feat-246-approach-spreading

### Summary
- Added local separation plus bounded open-angle arrival steering for eligible melee enemies during player pursuit.
- Preserved direct lone-enemy, barrier, HOLD, and forced attack-reacquisition movement.
- Added stable overlap symmetry breaking, gradual surround-arrival tuning, and forward-motion limits to both melee prefabs.

### New or Updated Tests
**EditMode**
- `EnemyApproachSpreadTests` and `EnemySurroundPrefabContractTests` — cover local separation, overlap fallback, arrival-band gap steering, distant direct pursuit, forward progress, speed caps, and melee prefab contracts.

**PlayMode**
- `EnemyApproachSpreadPlayTests` — covers early multi-path separation, direct spaced and lone pursuit, and direct #245 forced reacquisition.

### Notes
- EditMode 731/731 and PlayMode 59/59 passed in an isolated Unity project because the primary project was open in another editor instance.

## 2026-07-17 - fix-247-surround-eligibility

### Summary
- Restricted player-ring calculations to active, living, unrooted melee enemies targeting the player.
- Cleared stale angular gaps when enemies become ineligible or their controller is disabled.
- Added explicit melee-surround eligibility to both current enemy prefabs.

### New or Updated Tests
**EditMode**
- `EnemySurroundEligibilityTests`, `EnemyRingEligibilityTests`, and `EnemySurroundPrefabContractTests` — cover the eligibility matrix, filtered ring inputs, stale-gap cleanup, ranged-safe exclusion, and melee prefab contracts.

**PlayMode**
- `EnemyRingEligibilityPlayTests` — verifies mixed-target rings assign gaps only to eligible player-targeting melee enemies.

### Notes
- EditMode 724/724 and PlayMode 55/55 passed; manual validation confirmed existing eligible-enemy behavior remains unchanged.

## 2026-07-17 - fix-245-lock-melee-target

### Summary
- Locked each melee windup to the enemy's selected target.
- Cancelled escaped or deselected targets without damage or completed-attack cooldown.
- Resumed pursuit until collider-aware attack reach is reacquired.

### New or Updated Tests
**EditMode**
- `EnemyAttackTargetLockTests` — covers target identity, hit-time reach validation, and cancellation cooldown decisions.

**PlayMode**
- `EnemyAttackTargetLockPlayTests` — covers target-only damage, windup escape cancellation, pursuit resume, and fresh windup behavior.

### Notes
- EditMode 712/712 and PlayMode 54/54 passed in an isolated Unity project because the primary project was open in another editor instance.

## 2026-07-16 - fix-42-inside-barrier-damage-gate

### Summary
- Blocked protected barrier recipients at the final enemy damage boundary.
- Prevented player-targeted swings from damaging overlapping gates when both actors are inside.
- Preserved damage to non-barrier recipients and legitimate outside barrier attacks.

### New or Updated Tests
**EditMode**
- `EnemyAttackTargetTypeTests` — verifies a player-targeted attack blocks an inside barrier recipient without blocking non-barrier damage.

**PlayMode**
- N/A — N/A

### Notes
- EditMode and PlayMode test suites passed; manual validation confirmed enemies inside the castle do not damage barriers.

## 2026-07-16 - chore-44-remove-barrier-release-margin

### Summary
- Removed the unused barrier-specific release-margin configuration.
- Narrowed barrier hold decisions to barrier state, distance, and hold radius.
- Preserved controller release-margin hysteresis for non-barrier targets.

### New or Updated Tests
**EditMode**
- `EnemyBarrierHoldBehaviorTests` — covers broken, inside-radius, and outside-radius barrier hold decisions using the narrowed contract.

**PlayMode**
- N/A — N/A

### Notes
- EditMode and PlayMode test suites passed; manual validation confirmed enemy behavior is unchanged.

## 2026-07-16 - codex-ui-p5-meaningful-failure-closeout

### Summary
- Replaced duplicate Rise Again summary copy with the approved philosophical failure line.
- Preserved the Rise Again button as the single restart call to action.
- Strengthened fast-restart validation by exercising the actual failure-screen button.

### New or Updated Tests
**EditMode**
- `RunSummaryFormatterTests` — validates the approved philosophical line and guards against duplicate Rise Again copy.

**PlayMode**
- `DeathScreenRestartPlayTests` — validates the Rise Again button reloads MainPrototype with a fresh GameManager.

### Notes
- N/A

## 2026-07-15 - codex-ui-failure-run-summary

### Summary
- Added a complete run summary to the failure screen for issues #96 and #99.
- Added actual player damage-taken tracking, including lethal overkill clamping.
- Preserved the existing Rise Again restart flow while rendering the summary only on failure.

### New or Updated Tests
**EditMode**
- `RunStatsTests` — covers damage-taken initialization, accumulation, validation, and runtime relay tracking.
- `DamageTakenRunStatsRegressionTests` — validates lethal damage records only actual player health lost.
- `RunSummaryFormatterTests` — validates every run statistic maps to the failure summary.

**PlayMode**
- `DeathScreenRestartPlayTests` — validates the death screen displays populated run summary values and retains its tappable restart button.

### Notes
- N/A

## 2026-07-15 - codex-stats-run-stats-tracking-service

### Summary
- Added run counters for waves survived, enemies killed, damage dealt, repairs, health restored, and gold flow.
- Connected counters to confirmed gameplay outcomes through a lightweight runtime event relay.
- Excluded overheal and starting currency from earned run totals.

### New or Updated Tests
**EditMode**
- `RunStatsTests` — validates zero initialization, accumulation, wave survival, invalid amounts, and starting currency semantics.
- `HealthRunStatsRegressionTests` — validates health-restored stats count actual recovery without overheal.

**PlayMode**
- N/A — runtime wiring is installed by the existing `GameManager` without scene changes.

### Notes
- N/A

## 2026-07-14 - codex-spawning-lurker-enemy-type

### Summary
- Added the Lurker enemy type with a 2-frame sprite-sheet loop and dedicated prefab.
- Added Lurker balance at 35 health with slower movement than the current grunt enemy.
- Updated generated wave ramp scheduling to support multiple enemy type sequences in one wave, including explicit per-tier counts.
- Tuned the BasicSpawnSchedule Lurker ramp to start with 1 Lurker on wave 3 and add 1 every other wave.

### New or Updated Tests
**EditMode**
- `EnemySpriteSheetLoopTests` — validates runtime sheet slicing and looping.
- `WaveScheduleRampTests` — validates generated ramp waves split counts across multiple unlocked enemy types and honor explicit Lurker count ramps.
- `EnemySpawnerTests` — validates mixed wave sequences emit the correct enemy type requests.
- `EnemyBalanceTableTests` — validates Lurker balance and authored asset wiring.
- `PrefabSmokeTests` — validates the Lurker prefab runtime components and sprite-sheet loop wiring.
- `EnemySpawnScheduleAssetBalanceTests` — validates MainPrototype Lurker prefab mapping and BasicSpawnSchedule Lurker unlock.
- `PpuImportBaselineTests` — includes Lurker art in pixel import baseline coverage.

**PlayMode**
- `EnemySpawnerRunnerPlayTests` — validates the runtime spawner can emit both grunt and Lurker prefabs from one authored wave.

### Notes
- N/A

## 2026-07-14 - codex-backpack-move-to-vault

### Summary
- Added Backpack context menu transfer into the Castle Vault with the short `Vault` button label.
- Renamed the Vault-to-Backpack context action to `Backpack`.
- Gated Backpack-to-Vault transfer to inside-castle PreWave access while preserving Drop, Equip, and Vault panel actions.

### New or Updated Tests
**EditMode**
- `InventoryContextMenuControllerTests` — validates Backpack-to-Vault transfer, short action labels, menu close/retain behavior, and outside-castle / in-wave blocking.
- `InventoryPanelControllerTests` — validates Backpack row context menus expose the `Vault` action beside existing Drop and Equip actions.

**PlayMode**
- `InventoryPanelControllerPlayTests` — validates runtime Backpack context menu can move one item into the Vault when inside the castle during PreWave.

### Notes
- N/A

## 2026-07-11 - codex-vault-world-toggle

### Summary
- Added Vault world interaction toggling so the same input can open or close the dedicated Vault panel.
- Routed keyboard and touch-hold Vault input through the toggle path while preserving open-only `TryOpenVault` behavior.
- Kept the existing Vault close button, Backpack side-by-side behavior, and between-wave open gate intact.
- Fixed Vault context menus to use a Vault-scoped menu instance and clamp menu placement inside the owning panel.

### New or Updated Tests
**EditMode**
- `VaultWorldInteractionTests` - validates toggle open, toggle close, blocked open attempts, out-of-range close blocking, open-only compatibility, and touch fallback routing.
- `InventoryContextMenuControllerTests` - validates context menu placement clamps inside its parent panel.
- `VaultPanelControllerTests` - validates Vault menus do not steal Backpack menu ownership when both panels share a root.

**PlayMode**
- `VaultWorldInteractionPlayTests` - validates runtime toggle close/reopen while Backpack remains visible and the close button still closes the Vault.

### Notes
- Local EditMode runner could not execute because `ci/run-editmode.ps1` currently parse-fails in this shell.
- Local PlayMode runner could not execute because the configured Unity editor path was not found in this shell.

## 2026-07-10 - codex-vault-dedicated-panel

### Summary
- Added a dedicated Vault panel separate from the Inventory panel with a close button.
- Routed world Vault interaction to open the dedicated Vault panel while allowing Backpack to remain visible.
- Kept Shop and Backpack behavior unchanged, with club/dagger art metadata cleanup carried into the branch.

### New or Updated Tests
**EditMode**
- `VaultPanelControllerTests` — validates dedicated Vault rendering, close button behavior, blocked state, refresh, Vault context menu actions, and Vault weapon equip behavior.
- `VaultWorldInteractionTests` — validates world Vault interaction opens the dedicated panel and preserves Backpack visibility.
- `InventoryPanelControllerTests` — validates Inventory no longer owns the world Vault view.

**PlayMode**
- `VaultWorldInteractionPlayTests` — validates runtime Vault opening shows the dedicated Vault panel while Backpack remains visible and the close button closes it.
- `InventoryPanelControllerPlayTests` — validates Inventory panel no longer owns the world Vault view.

### Notes
- EditMode, PlayMode, and manual validation passed.
- Manual validation confirmed the dedicated Vault panel, Backpack side-by-side visibility, and X close button work in game.

## 2026-07-10 - codex-shop-purchasing

### Summary
- Added fixed-price Shop purchasing for Sword, Iron Club, and Health Potion.
- Routed weapon purchases to Backpack and Health Potion purchases to the active potion inventory.
- Wired Shop Buy buttons to existing gold spending with success and failure feedback.

### New or Updated Tests
**EditMode**
- `CastleShopCatalogTests` — validates fixed Shop prices for Sword, Iron Club, and Health Potion.
- `CastleShopPurchaseServiceTests` — validates purchase success, access denial, insufficient gold, invalid offers, full inventory, and item routing.
- `InventoryPanelControllerTests` — validates Buy affordance rendering, successful purchase feedback, and insufficient-gold feedback.

**PlayMode**
- `InventoryPanelControllerPlayTests` — validates runtime Shop offer price rendering, wave-start close behavior, and a buy flow that spends gold and adds a Backpack item.

### Notes
- EditMode, PlayMode, and manual validation passed.
- Manual validation confirmed purchasing works in game.

## 2026-07-10 - feat-p4-static-shop-catalog

### Summary
- Added a static castle shop catalog.
- Limited the initial Shop offers to Sword, Iron Club, and Health Potion.
- Updated the Shop tab to render catalog offers instead of placeholder supply rows.

### New or Updated Tests
**EditMode**
- `CastleShopCatalogTests` — validates the default catalog contains only Sword, Iron Club, and Health Potion.
- `InventoryPanelControllerTests` — validates the allowed Shop tab renders the approved catalog offers and excludes old placeholder rows.

**PlayMode**
- `InventoryPanelControllerPlayTests` — validates the runtime Shop smoke renders the approved catalog offers.

### Notes
- EditMode, PlayMode, and manual validation passed.
- Manual validation confirmed Shop renders Sword, Iron Club, and Health Potion only.

## 2026-07-10 - feat-p4-castle-shop-foundation

### Summary
- Added castle shop access policy for castle-region plus pre-wave gating.
- Made the Inventory panel Shop tab render blocked or prototype static shop rows based on access.
- Auto-closes an active Shop panel when the player exits the castle region or the wave phase changes to active.
- Stabilized Shop EditMode tests by using an editor-only castle-region test hook instead of private trigger message dispatch.

### New or Updated Tests
**EditMode**
- `CastleShopAccessPolicyTests` — validates shop access requires an available castle tracker, player inside castle, and pre-wave phase.
- `InventoryPanelControllerTests` — validates blocked Shop state, prototype Shop rows, and Shop auto-close on castle exit or wave start.

**PlayMode**
- `InventoryPanelControllerPlayTests` — smoke-validates Shop rows inside the castle and panel close on wave start.

### Notes
- EditMode, PlayMode, and manual validation passed.
- Manual validation confirmed Shop opens between waves only and shows a blocked message when unavailable.

## 2026-07-07 - feat-p4-castle-vault-world-interaction

### Summary
- Added world-gated Castle Vault interaction policy for range plus between-wave access.
- Added Vault outline state handling and a Vault prefab contract using the provided sprites.
- Replaced the Inventory panel Vault tab with an inert Shop tab and moved Vault viewing behind world interaction.
- Replaced Vault art with the larger 192px sprites and expanded the solid/interaction hitboxes to match the visible body.
- Added a top-center Start Wave HUD button and removed next-wave behavior from the upgrade menu close button.
- Changed Vault outlining from a scaled duplicate sprite to thin offset edge renderers.
- Moved mobile Vault hold targeting to the larger interaction trigger with explicit camera hit testing.
- Reduced the Vault solid blocker to the central body with rounded corners to prevent movement snagging while approaching it.
- Added overlap polling for Vault range detection so outline and interaction do not depend only on trigger callbacks.
- Synced the Vault interaction phase tracker into the inventory panel on open attempts so between-wave access cannot be denied by stale panel phase state.
- Restored full-opacity white/red Vault outline edge colors for clearer accessible/blocked feedback.
- Hardened Vault runtime reference discovery so scene instances can recover the active wave runner phase and sibling outline presenter.
- Increased the Vault outline offset to two pixels so the accessible white outline remains visible against the pale Vault sprite.
- Replaced shifted duplicate Vault outline rendering with a border-only sprite so accessible/blocked states render as a clean white/red rim.
- Added pointer fallback for Vault hold interaction so mobile/remote input paths that do not expose `Touchscreen.current` can still open the Vault.
- Stopped Vault open attempts from re-polling range during the input frame, preventing a visibly interactive Vault from being denied by a transient physics overlap miss.
- Wired the MainPrototype Vault instance directly to the scene InventoryPanelController and EnemySpawnerRunner so open attempts do not depend on runtime object search.
- Preserved explicitly assigned Vault phase trackers so tests and scene wiring cannot be overwritten by an ambient discovered runner in a different phase.
- Raised the Vault foundation sorting order above floor tilemaps while keeping it below player, enemies, and items.
- Added source-aware inventory context menus so Backpack and Vault actions can refresh without closing while the selected item still exists.
- Added Vault item actions for moving items to Backpack and equipping Vault weapons into active slots, returning displaced weapons to the Vault.
- Reduced the Vault solid blocker to avoid trapping the player against the Vault body while keeping the interaction trigger large.

### New or Updated Tests
**EditMode**
- `VaultInteractionPolicyTests` - validates range and wave-phase Vault access plus outline state selection.
- `VaultOutlinePresenterTests` - validates white/red/hidden outline rendering states and single border-sprite presentation.
- `VaultWorldInteractionTests` - validates world interaction opens Vault only in range and between waves, including overlap-polled range detection, current range-state opening, panel phase sync, discovered runner phase, explicit phase preservation, sibling outline lookup, and pointer fallback source contract.
- `VaultPrefabContractTests` - validates Vault prefab tag/layer, rounded blocking collider, foundation sorting above floor and below actors/items, trigger, outline components, mobile touch target, MainPrototype scene references, and 192px sprite import settings including the border-only outline sprite.
- `BackpackDropDirectionResolverTests` - guards the existing visual-forward drop convention.
- `InventoryContextMenuControllerTests` - validates source-aware Backpack/Vault menu actions, Vault-to-Backpack moves, and Vault weapon equip displacement.
- `InventoryPanelControllerTests` - validates world-open Vault rows, Vault context actions, context menu persistence across refresh while an item remains, and inert Shop tab behavior.
- `NextWaveHudButtonTests` - validates top-center runtime button placement, pre-wave visibility, and explicit wave start behavior.
- `TouchUIBindingsTests` - validates the upgrade menu close button closes without starting the next wave.
- `UpgradeMenuControllerTests` - validates upgrade menu close no longer starts the next wave.

**PlayMode**
- `VaultWorldInteractionPlayTests` - smoke-validates runtime world interaction opening the Vault panel.
- `InventoryPanelControllerPlayTests` - updates mid-wave Vault denial coverage for world-open flow.

### Notes
- Local `ci/run-editmode.ps1` validation was blocked by a PowerShell parse error in the existing script.
- Direct Unity 2022.3.62f2 EditMode validation was attempted with the CI entrypoint, but Unity hit the project-already-open crash path before NUnit XML was produced.
- Direct Unity Roslyn compile passes for `_Project.Gameplay` and `_Project.Tests.EditMode`; only the existing unused-field warning in `PlayerFacingPolicyResolver` remains.

## 2026-07-07 - feat-p4-backpack-context-actions

### Summary
- Added Backpack row context actions for weapon inventory flow.
- Added right-click and long-press row triggers that open Drop / Equip actions.
- Made each Backpack item row a full-width button that opens the context menu on click/tap.
- Added Equip slot choices for Main and Secondary active weapon slots.
- Added drop-to-world behavior that removes one Backpack weapon and spawns a pickup near the player.
- Fixed repeated Backpack weapon swaps after panel refresh by immediately deactivating stale runtime UI rows/menu buttons before deferred destruction.
- Updated dropped weapon behavior to slide farther forward with slight scatter using `LootSpillMotion` and block pickup for 5 seconds.
- Closed the open item context menu when Backpack rows refresh so round-end Backpack clearing cannot strand a stale menu.

### New or Updated Tests
**EditMode**
- `InventoryStateTests` - validates direct active weapon slot replacement.
- `BackpackWeaponEquipControllerTests` - validates Backpack weapon equip and displaced weapon return behavior.
- `BackpackWeaponEquipControllerTests` - validates repeated Backpack-to-active weapon swaps remain allowed.
- `BackpackItemDropControllerTests` - validates Backpack weapon drop spawning, outward slide motion, pickup cooldown blocking, and failure without mutation for unmapped ids.
- `InventoryContextMenuControllerTests` - validates Equip transforming into Main / Secondary and Main slot equip action.
- `InventoryPanelControllerTests` - validates button click and right-click opening Drop / Equip for weapon Backpack rows, repeated context-menu swaps after refresh, and closing stale menus when Backpack contents clear.

**PlayMode**
- `InventoryPanelControllerPlayTests` - smoke-validates Backpack context menu equip into the main weapon slot.

### Notes
- Local EditMode execution was attempted through the Unity CI entrypoint, but Unity exited through the project-already-open crash path and produced no NUnit XML.
- `git diff --check` passed.

## 2026-07-05 - issue-209-prototype-weapon-variety

### Summary
- Added iron club, club, and rusty dagger prototype weapon definitions using clean project-convention PNG sprites.
- Added basic weapon pickup prefabs and pickup visual sync from assigned item icons.
- Replaced the initial generated weapon art with hard-alpha pixel PNG source files.
- Expanded the basic weapon loot table and resolver wiring for prototype weapon variety.
- Fixed the player prefab resolver so equipped prototype weapons resolve stats and hand sprites.
- Wired PlayerWeaponController to the player-local weapon resolver to avoid resolver lookup ambiguity.
- Restored CastleFloorTilemap to the built-in sprite material after an unintended material override hid the floor.

### New or Updated Tests
**EditMode**
- ItemPickupVisualTests - pickup visuals render from assigned ItemDefinition icons.
- WeaponVarietyAssetTests - prototype weapon stats, prefabs, loot table entries, and sprite asset identity match the requested contracts.
- PrefabSmokeTests - player prefab resolves sword, iron club, club, and rusty dagger definitions from its local resolver.
- PickupMagnetPrefabContractTests - new weapon pickup prefabs keep magnet motion wiring.

**PlayMode**
- N/A - asset and prefab contract coverage only.

### Notes
- EditMode rerun after the resolver prefab fix was blocked because the Unity project was already open in another instance.
- Previous EditMode run before this resolver fix passed locally: 616 passed, 0 failed.
- PlayMode not required for this data/prefab slice.

## 2026-07-01 - feat(ui): inventory panel tabs

### Summary
- Added a display-only Inventory panel opened from a top-left Bag button.
- Added Backpack and Vault tabs with Backpack available mid-wave and Vault locked during waves.
- Anchored the new Bag button and Player health bar to the top-left HUD area for responsive placement.

### New or Updated Tests
**EditMode**
- `InventoryPanelControllerTests` - validates Backpack default/open behavior, Backpack and Vault row rendering, mid-wave Vault locking, change-event refresh, and top-left button anchoring.

**PlayMode**
- `InventoryPanelControllerPlayTests` - smoke-validates opening the Backpack panel and locking Vault access mid-wave.

### Notes
- Local Unity test execution was not run because the configured Unity editor path is missing and no Unity executable is available on PATH in this shell.

## 2026-07-01 - feat(inventory): transfer Backpack to Castle Vault

### Summary
- Added wave-end Backpack transfer into the persistent Castle Inventory vault.
- Cleared Backpack contents after successful transfer to keep it mid-wave-only storage.
- Wired the Player prefab with Castle Inventory and Backpack transfer components.

### New or Updated Tests
**EditMode**
- `BackpackVaultTransferTests` - validates transfer, Backpack clearing, empty no-op behavior, repeated transfer safety, active inventory preservation, and source configuration.
- `PrefabSmokeTests` - validates the Player prefab includes Backpack-to-vault transfer wiring.

**PlayMode**
- `BackpackVaultTransferPlayTests` - validates the wave-end event hook transfers Backpack contents into the Castle Vault.

### Notes
- Transfer listens to `EnemySpawnerRunner.OnWaveEnded` when a wave runner is present.

## 2026-07-01 - feat(inventory): add Backpack mid-wave carry

### Summary
- Added a temporary Backpack inventory for mid-wave overflow items.
- Routed overflow weapon pickups to Backpack when active weapon slots are full.
- Kept pickup magnet eligibility aligned with actual pickup eligibility so uncollectable items do not pull.

### New or Updated Tests
**EditMode**
- `BackpackInventoryStateTests` — validates capacity, valid adds, rejected inputs, removals, clear behavior, change events, sorted entries, and snapshot safety.
- `BackpackInventoryStateComponentTests` — validates stable component access and authored capacity.
- `ItemPickupTests` — validates active weapon slot priority, overflow weapon pickup into Backpack, and rejection when Backpack is full.
- `ItemPickupComponentTests` — validates component pickup consumption into Backpack and rejection when active inventory and Backpack are full.
- `PickupMagnetMotionTests` — validates overflow weapons attract only when Backpack can accept them.
- `PrefabSmokeTests` — validates the Player prefab includes authored Backpack inventory capacity.

**PlayMode**
- `PickupMagnetSweepPlayTests` — validates an overflow weapon pickup is collected into Backpack when active weapon slots are full.

### Notes
- Potion stack limits remain unchanged and do not overflow to Backpack in this slice.

## 2026-07-01 - feat(inventory): add Castle Inventory persistent vault

### Summary
- Added a separate Castle Inventory vault state for persistent count-based item storage.
- Added a lightweight runtime component wrapper without changing active player inventory behavior.
- Covered valid adds, rejected inputs, removals, event emission, sorted entries, and snapshot safety.

### New or Updated Tests
**EditMode**
- `CastleInventoryStateTests` — validates vault item counts, invalid input rejection, removal semantics, change events, sorted entries, and snapshot safety.
- `CastleInventoryStateComponentTests` — validates stable component access to the vault state.

**PlayMode**
- N/A — data-model slice only; no scene or prefab runtime path changed.

### Notes
- Active `InventoryState` combat inventory remains unchanged.

## 2026-06-26 - fix(barrier): allow repair below max health with cooldown

### Summary
- Added damaged-barrier repair coverage so barriers below max health are repairable before breaking.
- Added player repair cooldown coverage to prevent repeated repair attempts until the cooldown expires.
- Routed repair cooldown tuning through the player balance table and balance applier.

### New or Updated Tests
**EditMode**
- `BarrierHealthTests` - validates damaged repair, full-health no-op repair, and repaired event behavior.
- `PlayerRepairControllerTests` - validates repair sensor detection, repair cooldown start, full-health no-op, and cooldown expiry.
- `PlayerBalanceTableTests` - validates repair cooldown defaults, clamping, and project asset tuning.
- `PlayerBalanceApplierTests` - validates repair cooldown application and missing-table preservation.
- `TouchRepairButtonTests` - validates repair visibility for damaged barriers instead of broken-only barriers.
- `PrefabSmokeTests` - validates the authored Player prefab repair cooldown.

**PlayMode**
- `BarrierRepairOverlapIntegrationPlayTests` - validates overlap resolution still runs from a damaged repair flow.

### Notes
- N/A

## 2026-06-21 - feat(loot): add magnetic pickup attraction

### Summary
- Added wall-agnostic pickup attraction with a moderate in-wave radius and faster extended sweep between waves.
- Routed standard and sweep range/speed tuning through the shared economy balance table.
- Preserved spill motion, pickup delay, inventory eligibility, and existing collection behavior while removing recurring eligibility allocations.

### New or Updated Tests
**EditMode**
- `PickupMagnetFieldTests` - validates standard and sweep profiles, wave transitions, range checks, and normalized tuning.
- `PickupMagnetMotionTests` - validates wall-agnostic attraction, spill coordination, and full weapon inventory regression behavior.
- `PickupMagnetPrefabContractTests` - validates player balance configuration and magnet motion on gold, potion, and weapon prefabs.
- `ItemPickupComponentTests` - validates allocation-free cached eligibility, cache invalidation, pickup consumption, full inventory, and delay behavior.

**PlayMode**
- `PickupMagnetSweepPlayTests` - validates wave-end collection of distant gold through a wall into player inventory.

### Notes
- Unity assemblies compiled successfully; allocation and magnet test execution awaits the next Test Runner pass.

## 2026-06-20 - fix(visual): preserve tower and projectile render order

### Summary
- Rendered Tower platform and top visuals above barrier and castle wall stonework.
- Kept tower-fired arrows above Tower visuals until they clear the launch area, then moved them below Enemy sprites during flight.
- Disabled launch-distance updates after the one-time sorting transition and for non-Tower projectiles.

### New or Updated Tests
**EditMode**
- `TowerRuntimeContractTests` - validates Tower platform and top sorting orders.
- `TowerAttackControllerTests` - validates tower-fired arrows begin above Tower visuals.
- `ProjectileArrowPrefabContractTests` - validates flight ordering below enemies and the one-way Tower launch transition.

**PlayMode**
- `TowerSpawnInitPlayTests` - validates tower-fired arrows return to flight order after clearing the Tower.

### Notes
- Gameplay, EditMode, and PlayMode assemblies compile successfully; full Unity test-runner validation remains required.

## 2026-06-20 - fix(barrier): preserve gate baseline and wall occlusion

### Summary
- Rendered Player and Enemy sprites below barrier Wall/Arch layers and the castle wall tilemap.
- Kept pickup sprites visible above barrier and castle wall stonework.
- Prevented repeated hits and repair from leaving the Gate shake target offset from its authored position.

### New or Updated Tests
**EditMode**
- `BarrierPrefabVisualContractTests` - validates Player/Enemy wall occlusion and pickup visibility above stonework.
- `CastleWallTilemapColliderContractTests` - validates castle walls render above Player and Enemy prefabs.
- `BarrierHealthTests` - verifies repair raises the Gate reset notification.

**PlayMode**
- `BarrierHitShakeVisualSegmentPlayTests` - verifies repeated hits and repair restore the stable Gate baseline.
- `CastleTilemapRuntimeContractsPlayTests` - validates runtime castle wall sorting order.

### Notes
- Unity rerun was blocked by the managed licensing/escalation wrapper after the red sorting-contract run; final EditMode and PlayMode reruns remain required.

## 2026-06-20 - fix(barrier): isolate damage shake to visual gate segment

### Summary
- Split each directional barrier visual into aligned Ground, Gate, Wall, and Arch render layers.
- Isolated hit shake and broken-state visibility to the collider-free Gate visual while root physics remains stationary.
- Ordered barrier and enemy renderers as Ground, Gate, Wall, Enemy, then Arch.

### New or Updated Tests
**EditMode**
- `BarrierPrefabVisualContractTests` - validates layered sprite assignments, hierarchy, sorting, physics isolation, and enemy interleaving.
- `BarrierVisualBindingTests` - validates directional sprite-set selection for layered barrier visuals.
- `BarrierHitShakeTests` - validates the configured Gate shake target remains separate from the barrier root.
- `BarrierHealthTests` - verifies breaking a layered barrier disables only the Gate renderer.
- `MainPrototypeBarrierAssemblyIntegrationTests` - validates generated barriers expose four assigned visual layers.

**PlayMode**
- `BarrierHitShakeVisualSegmentPlayTests` - verifies Gate shake does not move the barrier root or collider and restores its baseline.
- `CastleTilemapRuntimeContractsPlayTests` - validates generated runtime barriers use four assigned child renderers.

### Notes
- Full EditMode and PlayMode suites passed in Unity 2022.3.62f2.

## 2026-06-13 - fix(defense): skip pulse knockback while rooted

### Summary
- Skipped barrier pulse knockback application for enemies currently held by root effects.
- Preserved barrier pulse knockback after a rooted enemy is released while the pulse remains active.
- Added regression coverage for trap-rooted enemies interacting with barrier pulse knockback.

### New or Updated Tests
**EditMode**
- `BarrierPulseEmitterTests` - verifies rooted enemies receive no barrier pulse knockback until root ends.

**PlayMode**
- N/A - N/A

### Notes
- EditMode and PlayMode passed per user validation.

## 2026-06-12 - fix(defense): catch released enemies on armed traps

### Summary
- Routed Bear Trap enter and stay trigger checks through the same guarded activation path.
- Allowed released enemies still overlapping an armed trap to be caught after root expires.
- Preserved the rule that currently rooted enemies cannot consume additional traps.

### New or Updated Tests
**EditMode**
- `BearTrapTriggerTests` - verifies trigger stay ignores rooted enemies, then catches the same enemy after root release while overlapping another armed trap.

**PlayMode**
- N/A - N/A

### Notes
- N/A

## 2026-06-12 - fix(visual): use bear trap sprite for placement preview

### Summary
- Updated placement preview sprite resolution to prefer the selected Bear Trap prefab open sprite.
- Preserved placeholder preview fallback for placeables without authored sprite art.
- Added regression coverage for Bear Trap ghost preview art selection.

### New or Updated Tests
**EditMode**
- `BearTrapPlacementPrototypeTests` - verifies placement preview uses the selected Bear Trap open sprite before placeholder art.

**PlayMode**
- N/A - N/A

### Notes
- N/A

## 2026-06-12 - feat(visual): add bear trap animation states

### Summary
- Added the authored Bear Trap PNG sprite sheet under project art assets.
- Imported the sheet as open, close, and closed sprite frames.
- Wired the Bear Trap prefab visual state to play the close frame animation and remain closed when spent.

### New or Updated Tests
**EditMode**
- `BearTrapPlacementPrototypeTests` - verifies the authored Bear Trap sprite sheet, frame names, prefab sprite references, and three-frame close animation contract.

**PlayMode**
- N/A - N/A

### Notes
- Local Unity validation was not rerun in this shell because batchmode was blocked by Unity licensing IPC timeout earlier in the session.

## 2026-06-12 - test(defense): stabilize trap occupancy release test

### Summary
- Added an explicit idempotent release method to placed-object occupancy leases.
- Updated the trap replacement regression to release the lease deterministically in EditMode.
- Kept OnDestroy release behavior intact for runtime trap cleanup.

### New or Updated Tests
**EditMode**
- `BearTrapPlacementPrototypeTests` - verifies placed traps carry an occupancy lease and release their cell before replacement.

**PlayMode**
- N/A - N/A

### Notes
- N/A

## 2026-06-12 - fix(defense): release deleted trap occupancy

### Summary
- Added occupancy release support to the castle placement occupancy map.
- Attached a placement occupancy lease to placed objects so destroyed traps free their grid cell.
- Added regression coverage for replacing a trap after its placed instance is destroyed.

### New or Updated Tests
**EditMode**
- `BearTrapPlacementPrototypeTests` - verifies destroyed placed traps release their occupied cell for replacement.

**PlayMode**
- N/A - N/A

### Notes
- Local Unity validation was not rerun in this shell because batchmode was blocked by Unity licensing IPC timeout earlier in the session.

## 2026-06-11 - fix(defense): lock trapped enemies to trap tile

### Summary
- Locked trapped enemies to the triggering Bear Trap position for the hold duration.
- Prevented already-rooted enemies from triggering additional Bear Traps.
- Disabled enemy attack damage while the enemy is rooted by trap behavior.

### New or Updated Tests
**EditMode**
- `BearTrapTriggerTests` - verifies trap-position locking, no chained trap activation by rooted enemies, and independent activation by other enemies.
- `EnemyAttackTests` - verifies rooted enemies do not deal attack damage.

**PlayMode**
- N/A - N/A

### Notes
- Local Unity validation was not rerun in this shell because batchmode was blocked by Unity licensing IPC timeout earlier in the session.

## 2026-06-11 - fix(defense): delete expired bear traps

### Summary
- Changed Bear Trap wave lifetime default to delete expired traps at wave end.
- Lowered Bear Trap visual sorting so enemies render above traps.
- Added contract coverage for delete-after-wave behavior and trap render order.

### New or Updated Tests
**EditMode**
- `BearTrapTriggerTests` - verifies default disappear policy deletes the trap after wave lifetime while reset policy remains supported.
- `BearTrapPlacementPrototypeTests` - verifies Bear Trap prefab deletes expired traps and renders below enemies.

**PlayMode**
- N/A - N/A

### Notes
- Local Unity validation was not rerun in this shell because batchmode was blocked by Unity licensing IPC timeout earlier in the session.

## 2026-06-11 - feat(defense): add bear trap trigger behavior

### Summary
- Added Bear Trap runtime trigger behavior for one-enemy activation.
- Added tunable damage, hold duration, wave lifetime, and wave-end reset/disappear policy defaults.
- Added enemy root receiver support and a movement regression guard for rooted enemies.

### New or Updated Tests
**EditMode**
- `BearTrapTriggerTests` - verifies default tuning, one-enemy damage/root/spend behavior, spent-trap ignore behavior, wave-end reset, root release timing, and rooted enemy movement guard.
- `BearTrapPlacementPrototypeTests` - verifies Bear Trap prefab runtime trigger wiring.

**PlayMode**
- N/A - N/A

### Notes
- Local Unity EditMode run was blocked by Unity licensing IPC timeout before NUnit XML was produced.

## 2026-06-08 - feat(defense): validate trap placement surface

### Summary
- Resolved Bear Trap placement surface from the castle region collider instead of assuming every cursor cell is outside ground.
- Rejected locked trap placement inside the castle region and on wall/barrier blockers while keeping outside-ground placement valid.
- Wired MainPrototype placement validation to the existing CastleRegion collider.

### New or Updated Tests
**EditMode**
- `BearTrapPlacementPrototypeTests` - verifies castle-region surface resolution, inside-castle rejection, wall/barrier blocker rejection, outside-ground acceptance, and MainPrototype validation wiring.

**PlayMode**
- N/A - N/A

### Notes
- Local batchmode run was blocked because the project was already open in another Unity instance; awaiting validation.

## 2026-06-08 - feat(defense): add confirm cancel placement loop

### Summary
- Added locked-target Bear Trap placement with Confirm and Cancel controls.
- Kept placement mode active after each confirmed trap so another trap can be placed.
- Reopened the upgrade menu on the Defense tab when placement is canceled, without leaving touch or PC attack stuck active.
- Added player attack cleanup that clears held fire, resets active attack loops, and syncs attack presentation without locking movement.
- Limited placement pointer blocking to Confirm/Cancel controls so full-screen touch zones do not hide the preview.

### New or Updated Tests
**EditMode**
- `BearTrapPlacementPrototypeTests` - verifies locked target confirmation, repeat placement, occupied-cell rejection, cancel callback behavior, touch/PC attack release on cancel, and no global UI hit-test blocking for preview.
- `PlayerAttackInputTests` - verifies player attack cleanup clears held fire and active attack loop state.
- `UpgradeMenuControllerTests` - verifies hiding for placement does not start the wave and can reopen the menu.

**PlayMode**
- N/A - N/A

### Notes
- Local batchmode run was blocked because the project was already open in another Unity instance; awaiting validation.

## 2026-06-08 - feat(defense): move trap placement into upgrade flow

### Summary
- Moved Bear Trap placement entry into the Defense tab of the upgrade menu.
- Removed standalone Bear Trap button creation from the placement controller.
- Added a menu hide path that starts placement without starting the next wave.

### New or Updated Tests
**EditMode**
- `UpgradeMenuListViewTowerRowsTests` - verifies the Defense tab renders Bear Trap and delegates placement to the world placement controller.
- `BearTrapPlacementPrototypeTests` - verifies the standalone Bear Trap button path is no longer present.

**PlayMode**
- N/A - N/A

### Notes
- Local batchmode run was blocked because the project was already open in another Unity instance; awaiting validation.

## 2026-06-08 - feat(ui): add castle and defense upgrade tabs

### Summary
- Added Castle and Defense tabs to the upgrade menu while keeping the same menu frame.
- Moved barrier upgrade rows under Castle and tower build/upgrade rows under Defense.
- Enlarged per-tab row content and extracted tab strip behavior away from row rendering.

### New or Updated Tests
**EditMode**
- `UpgradeMenuListViewTowerRowsTests` - verifies Castle tab barrier rows, Defense tab tower rows, and existing barrier/tower actions after tab split.

**PlayMode**
- `TowerBuildUpgradeMenuVerticalSlicePlayTests` - switches to the Defense tab before tower build/upgrade vertical-slice assertions.

### Notes
- Local batchmode run was blocked because the project was already open in another Unity instance; awaiting validation.

## 2026-06-08 - feat(defense): add bear trap placement prototype

### Summary
- Added a minimal bear trap placeable definition and prefab using a replaceable placeholder sprite visual.
- Added runtime placement rules for 1x1 outside-ground snapping and occupied-footprint rejection.
- Wired MainPrototype with a prototype bear trap placement controller that creates a simple runtime select button.

### New or Updated Tests
**EditMode**
- `BearTrapPlacementPrototypeTests` - verifies bear trap definition authoring, outside-ground validation, occupied rejection, grid snapping, prefab visual swap contract, and MainPrototype controller wiring.

**PlayMode**
- N/A - N/A

### Notes
- EditMode passing per user validation; local batchmode run was blocked because the project was already open in another Unity instance.

## 2026-06-08 - feat(world): add castle and defense placeable definitions

### Summary
- Added placeable object definitions for authored Castle and Defense placement data.
- Authored current barrier gate and tower placeables as 3x3 footprints.
- Added placement surface authoring while keeping overlap and tower wall-mount rules out of this data-only slice.

### New or Updated Tests
**EditMode**
- `PlaceableObjectDefinitionTests` - verifies required authoring fields, current categories, placement surfaces, explicit footprint dimensions, invalid footprints, and project asset wiring.

**PlayMode**
- N/A - N/A

### Notes
- Awaiting user-run EditMode validation.

## 2026-06-07 - feat(world): add grid footprint placement contract

### Summary
- Added an explicit grid footprint value contract for declared placement dimensions.
- Routed castle placement and occupancy checks through reusable footprint APIs.
- Preserved existing 3x3 barrier placement wrappers while adding 1x1 and declared-footprint coverage.

### New or Updated Tests
**EditMode**
- `CastleModulePlacementRulesTests` - verifies declared footprint cell enumeration, invalid footprint rejection, 1x1 placement, overlap blocking, off-lattice rejection, and existing 3x3 barrier placement behavior.

**PlayMode**
- N/A - N/A

### Notes
- Unity EditMode run attempted on 2026-06-07 but failed before tests due to Unity licensing IPC timeout; no NUnit XML was produced.

## 2026-06-05 - refactor(player): add player baseline tuning table

### Summary
- Added authored player baseline fields to PlayerBalanceTable.
- Added a PlayerBalanceTable project asset and wired it through GameBalanceStation.
- Routed player max HP, move speed, and repair range through a PlayerBalanceApplier while preserving instant full barrier repair.

### New or Updated Tests
**EditMode**
- `PlayerBalanceTableTests` - verifies defaults, clamping, and project asset wiring.
- `PlayerBalanceApplierTests` - verifies baseline application and missing-table fallback behavior.

**PlayMode**
- N/A - N/A

### Notes
- EditMode and PlayMode tests passed per user on 2026-06-07.

## 2026-06-05 - refactor(loot): weight basic potion drop amounts

### Summary
- Updated LootTable_PotionBasic so successful potion drops usually grant 1 potion.
- Added a lower-weight 2-potion outcome for occasional bonus potion drops.
- Kept enemy loot profile roll counts unchanged so potion drops remain one table result per enemy.

### New or Updated Tests
**EditMode**
- `LootTableRngTests` - verifies PotionBasic asset weights 1 potion at 8 and 2 potions at 2.

**PlayMode**
- N/A - N/A

### Notes
- N/A

## 2026-06-05 - refactor(enemy): add enemy stat and reward tuning table

### Summary
- Added authored enemy stat rows through EnemyBalanceTable with the current grunt baseline.
- Added an EnemyLootProfile asset for the current grunt reward package while preserving existing loot table mappings.
- Routed spawned enemies through an EnemyBalanceApplier so stats, XP, and loot mappings resolve from the central GameBalanceStation.

### New or Updated Tests
**EditMode**
- `EnemyBalanceTableTests` - verifies defaults, clamping, lookup behavior, and project asset wiring.
- `EnemyBalanceApplierTests` - verifies stat and reward application plus missing-row fallback behavior.

**PlayMode**
- N/A - N/A

### Notes
- Unity EditMode run attempted on 2026-06-05 but failed before tests due to Unity licensing IPC timeout; no NUnit XML was produced.

## 2026-05-26 - refactor(waves): add wave pacing and difficulty tuning table

### Summary
- Added authored shared wave default fields to WaveBalanceTable.
- Added the WaveBalanceTable project asset and wired it through GameBalanceStation.
- Routed EnemySpawnScheduleAsset wave defaults through the balance table when assigned while preserving fallback behavior.
- Added active generated wave builds so the wave table can define count scaling presets like 5 enemies on wave 1 and 15 on wave 2.
- Wired MainPrototype's EnemySpawnerRunner to the central balance station so active wave builds reach runtime.
- Updated the spawner runner gate so generated wave schedules use the wave runtime even when no authored waves exist.
- Tuned the default generated wave gap to 8 seconds to leave time for post-wave loot collection.

### New or Updated Tests
**EditMode**
- `WaveBalanceTableTests` - verifies defaults, clamping, seed resolution, and project asset wiring.
- `WaveScheduleRuntimeTests` - verifies fallback and generated ramp waves use provided shared defaults, active build enemy count scaling, max count capping, generated-schedule availability, and no-build fallback behavior.
- `EnemySpawnScheduleAssetBalanceTests` - verifies balance-table routing, runner station override behavior, active build count generation, MainPrototype station wiring, and no-table fallback defaults.

**PlayMode**
- N/A - N/A

### Notes
- EditMode and PlayMode tests passed per user on 2026-05-30.

## 2026-05-26 - refactor(economy): add reward and cost tuning table

### Summary
- Added authored starting currency fields to EconomyBalanceTable.
- Added the EconomyBalanceTable project asset and wired it through GameBalanceStation.
- Routed InventoryStateComponent starting currency through the economy table when assigned.

### New or Updated Tests
**EditMode**
- `EconomyBalanceTableTests` - verifies defaults, clamping, inventory starting currency routing, fallback behavior, and project asset wiring.

**PlayMode**
- N/A - N/A

### Notes
- EditMode and PlayMode tests passed per user on 2026-05-26.

## 2026-05-26 - fix(loot): clarify reward item handling and enemy drops

### Summary
- Added an explicit GoldDefinition type for currency reward items.
- Updated loot spawn classification so unsupported generic items no longer become gold pickups.
- Migrated the authored Gold_1 reward asset to GoldDefinition while preserving existing loot table references.

### New or Updated Tests
**EditMode**
- `LootSpawnPolicyTests` - verifies weapon, potion, gold, legacy gold, unsupported generic item, and authored gold asset classification.
- `LootDropperTests` - verifies gold splitting uses the explicit gold reward type.

**PlayMode**
- N/A - N/A

### Notes
- EditMode tests passed in Unity editor validation on 2026-05-26.

## 2026-05-25 - refactor(barrier): route barrier tuning through balance tables

### Summary
- Added authored barrier health and upgrade-cost fields to BarrierBalanceTable.
- Routed BarrierUpgradeConfig through GameBalanceStation barrier tables when assigned.
- Added a BarrierBalanceTable asset wired through the central station while preserving current barrier asset tuning.

### New or Updated Tests
**EditMode**
- `BarrierBalanceTableTests` - verifies default table values, clamping, tier formulas, and project asset wiring.
- `BarrierUpgradeTests` - verifies barrier health, upgrade costs, and purchase spending resolve through the assigned balance table.

**PlayMode**
- N/A - N/A

### Notes
- EditMode tests passed in Unity editor validation on 2026-05-25.

## 2026-05-24 - refactor(tower): route tower tuning through balance tables

### Summary
- Added authored tower build, combat, and upgrade-track tuning fields to TowerBalanceTable.
- Mirrored current tower build cost, health, damage, cooldown, range, and base upgrade track defaults.
- Added EditMode coverage for tower table defaults, clamping, track lookup, and resolved base values.
- Routed tower build and upgrade configs through GameBalanceStation tower tables when assigned.
- Added central GameBalanceStation and TowerBalanceTable assets for authored tower tuning.

### New or Updated Tests
**EditMode**
- `TowerBalanceTableTests` - verifies current tower defaults, scalar clamping, track contracts, base resolved values, and project asset wiring.
- `TowerBuildControllerTests` - verifies build config resolves authored values from the station tower table.
- `TowerUpgradeControllerTests` - verifies tower upgrades apply station table tracks to runtime stats and costs.

**PlayMode**
- N/A - N/A

### Notes
- EditMode tests passed in Unity editor validation on 2026-05-25.

## 2026-05-20 - refactor(balance): introduce central tuning station

### Summary
- Added a typed GameBalanceStation ScriptableObject for category balance table references.
- Added empty category table assets for tower, barrier, wave, enemy, player, and economy tuning.
- Added EditMode coverage for assigned references, partial station setup, and ScriptableObject table contracts.

### New or Updated Tests
**EditMode**
- `GameBalanceStationTests` - verifies typed category references, optional missing tables, and ScriptableObject table contracts.

**PlayMode**
- N/A - N/A

### Notes
- EditMode tests passed in Unity editor validation.

## 2026-05-15 - test(tower): cover upgrade flow and scene wiring

### Summary
- Added PlayMode smoke coverage for Tower prefab upgrade controller/config runtime initialization.
- Updated the MainPrototype tower build vertical slice to upgrade a built tower through the menu.
- Kept duplicate-build regression coverage after tower upgrade actions are exposed.

### New or Updated Tests
**EditMode**
- N/A

**PlayMode**
- `TowerSpawnInitPlayTests` - verifies instantiated Tower prefab includes upgrade support and base config values apply.
- `TowerBuildUpgradeMenuVerticalSlicePlayTests` - verifies menu build flow exposes and invokes a built tower damage upgrade.

### Notes
- Not run locally; Unity batchmode could not connect to the local licensing client and produced no NUnit XML.

## 2026-05-15 - feat(ui): expose tower upgrade track actions

### Summary
- Replaced occupied tower plot rows with per-track tower upgrade actions.
- Added compact DMG, RATE, HP, and RNG buttons for supported tower upgrade tracks.
- Refreshed occupied tower details from live tower runtime, attack, and targeting values.

### New or Updated Tests
**EditMode**
- `UpgradeMenuListViewTowerRowsTests` - verifies occupied tower rows expose track buttons and invoke the tower upgrade controller.

**PlayMode**
- N/A - N/A

### Notes
- Not run locally; Unity batchmode could not connect to the local licensing client and produced no NUnit XML.

## 2026-05-15 - feat(tower): add per-instance upgrade tracks

### Summary
- Added per-prefab tower upgrade configs with independent damage, fire-rate, health, and range tracks.
- Added per-instance tower upgrade state and controller purchase/application flow.
- Wired the base tower prefab to authored upgrade rules.

### New or Updated Tests
**EditMode**
- `TowerUpgradeControllerTests` - verifies track purchases, formulas, limits, pre-wave gating, feedback, and per-instance divergence.
- `TowerRuntimeContractTests` - verifies the base tower prefab includes upgrade controller/config wiring.

**PlayMode**
- N/A - N/A

### Notes
- Not run locally; Unity batchmode could not connect to the local licensing client and produced no NUnit XML.

## 2026-05-15 - refactor(tower): move targeting range to instances

### Summary
- Moved tower min/max targeting range onto TowerTargetingController instance state.
- Kept TowerTargetingProfile focused on shared targeting rules.
- Added regression coverage for towers sharing a profile while using different instance ranges.

### New or Updated Tests
**EditMode**
- `TowerTargetingControllerTests` - verifies instance-owned range, min/max filtering, and shared-profile range independence.
- `TowerRuntimeContractTests` - verifies the base tower prefab serializes valid instance targeting range.

**PlayMode**
- N/A - N/A

### Notes
- Not run locally; Unity batchmode could not connect to the local licensing client and produced no NUnit XML.

## 2026-05-10 - feat(ui): add wave count HUD indicator

### Summary
- Added a focused WaveCountHud binder for displaying the current wave index.
- Wired the HUD to initialize from IWaveIndexProvider and refresh from EnemySpawnerRunner wave-start events.
- Added the MainPrototype wave counter HUD beside the potion slot using the pixel frame, subtle dark backing, and BoldPixels font.

### New or Updated Tests
**EditMode**
- `WaveCountHudTests` - verifies provider initialization, wave-start updates, missing-provider fallback, and MainPrototype HUD wiring.

**PlayMode**
- `WaveCountHudPlayTests` - verifies the HUD refreshes when the runner starts a new wave.

### Notes
- N/A

## 2026-05-10 - fix(projectile): prevent lingering arrow double hits

### Summary
- Guarded projectile trigger handling after impact so lingering arrows cannot hit again.
- Preserved arrow linger and embed visuals without changing launch or damage contracts.
- Added regression coverage for queued trigger callbacks after impact.

### New or Updated Tests
**EditMode**
- `ProjectileRuntimeTests` - verifies impacted projectiles ignore later trigger callbacks without damage or feedback.

**PlayMode**
- N/A - N/A

### Notes
- Not run locally; CI should be rerun after runner Unity account reactivation.

## 2026-05-06 - feat(weapon): add reusable crossbow fire animation

### Summary
- Moved the crossbow fire sheet into shared weapon art and sliced it into six 64x64 frames.
- Added a reusable sprite-frame weapon fire animation player with cooldown-scaled playback speed.
- Wired the base tower to trigger the shared crossbow fire animation from TowerAttackController.Fired.

### New or Updated Tests
**EditMode**
- `WeaponFireAnimationPlayerTests` - verifies cooldown-based animation playback speed scaling and clamps.
- `TowerRuntimeContractTests` - verifies Tower prefab crossbow fire animation wiring and six assigned frames.

**PlayMode**
- N/A - N/A

### Notes
- Not run locally; Unity EditMode tests should be run in the editor.

## 2026-05-03 - fix(projectile): tune arrow embed distance

### Summary
- Increased Projectile_Arrow impact embed distance so hits visibly connect with enemies.
- Kept the existing arrow impact linger and shared enemy hit flash behavior.
- Updated prefab contract coverage to protect the tuned embed threshold.

### New or Updated Tests
**EditMode**
- `ProjectileArrowPrefabContractTests` - verifies Projectile_Arrow has at least 0.45 world units of impact embed distance.

**PlayMode**
- N/A - N/A

### Notes
- Not run locally; Unity EditMode tests should be run in the editor.

## 2026-05-03 - fix(projectile): embed arrows on impact

### Summary
- Added opt-in projectile impact embed distance so arrows nudge forward when they hit.
- Increased Projectile_Arrow impact linger so enemy hits remain readable.
- Kept projectile damage and hit flash behavior unchanged.

### New or Updated Tests
**EditMode**
- `ProjectileRuntimeTests` - verifies impact embed moves a projectile forward along its launch direction.
- `ProjectileArrowPrefabContractTests` - verifies Projectile_Arrow has nonzero impact embed and linger tuning.

**PlayMode**
- N/A - N/A

### Notes
- Not run locally; Unity EditMode tests should be run in the editor.

## 2026-05-03 - fix(projectile): flash enemies on arrow hit

### Summary
- Reused the existing PlayerHitEnemy feedback cue when projectiles damage enemies.
- Wired Projectile_Arrow to the shared enemy hit flash feedback channel.
- Added a short Projectile_Arrow impact linger so hits read more clearly.

### New or Updated Tests
**EditMode**
- `ProjectileRuntimeTests` - verifies projectile hits damage a target and raise the shared PlayerHitEnemy feedback cue.
- `ProjectileArrowPrefabContractTests` - verifies Projectile_Arrow is wired to enemy hit flash feedback and has a short impact linger.

**PlayMode**
- N/A - N/A

### Notes
- Not run locally; Unity EditMode tests should be run in the editor.

## 2026-05-03 - fix(projectile): render arrows above tower

### Summary
- Raised the arrow projectile sorting order so fired arrows render above the crossbow tower top.
- Preserved the tower foundation and crossbow sorting order from the base tower visual pass.
- Added prefab contract coverage for arrow projectile render layering.

### New or Updated Tests
**EditMode**
- `ProjectileArrowPrefabContractTests` - verifies `Projectile_Arrow` renders above the base tower crossbow top.

**PlayMode**
- N/A - N/A

### Notes
- Not run locally; Unity EditMode tests should be run in the editor.

## 2026-05-03 - fix(tower): render foundation above wall

### Summary
- Raised the base tower foundation sorting order so it renders on top of the castle wall.
- Raised the crossbow tower top sorting order so it remains above the foundation baseplate.
- Updated prefab contract coverage to protect the foundation and tower top layering.

### New or Updated Tests
**EditMode**
- `TowerRuntimeContractTests` - verifies the Tower foundation renders above the barrier wall baseline and the arrow top renders above the foundation.

**PlayMode**
- N/A - N/A

### Notes
- Not run locally; Unity EditMode tests should be run in the editor.

## 2026-05-03 - feat(tower): assign base tower sprites

### Summary
- Assigned the base tower prefab to the authored arrow tower top sprite.
- Assigned the base tower prefab to the authored tower foundation sprite.
- Added prefab contract coverage for the approved base tower sprites and aligned fire point.

### New or Updated Tests
**EditMode**
- `TowerRuntimeContractTests` - verifies the Tower prefab uses `Tower_Arrow`, uses `Tower_Foundation`, rotates the arrow art to local forward, and keeps FirePoint forward on the aim axis.

**PlayMode**
- N/A - N/A

### Notes
- Not run locally; Unity EditMode tests should be run in the editor.

## 2026-05-02 - fix(tower): align projectile spawn with aim

### Summary
- Added one-shot projectile rotation so arrows visually follow their launch direction.
- Added a FirePoint under the tower AimPivot so projectile spawn position follows tower aim.
- Added a -45 degree projectile visual offset for the diagonal arrow sprite.

### New or Updated Tests
**EditMode**
- `TowerAttackControllerTests` - verifies projectile rotation aligns to launch direction with visual offset.
- `TowerRuntimeContractTests` - verifies FirePoint parenting, attack fire point assignment, and arrow visual offset.

**PlayMode**
- N/A - N/A

### Notes
- Not run locally; user will run tests manually in Unity and CI before merge.

## 2026-05-02 - feat(tower): wire projectile attack prefab

### Summary
- Wired the base Tower prefab with the tested tower attack controller.
- Assigned the reusable arrow projectile prefab, AimPivot fire point, per-tower attack stats, and Enemies target mask.
- Added prefab contract coverage for base arrow tower attack wiring.

### New or Updated Tests
**EditMode**
- `TowerRuntimeContractTests` - verifies the Tower prefab serializes attack controller, targeting reference, projectile prefab, fire point, attack stats, and Enemies target mask.

**PlayMode**
- N/A - N/A

### Notes
- EditMode passed per user in Unity.
- Manual play validated towers fire projectiles and damage enemies; projectile spawn/aim alignment remains a follow-up fix.

## 2026-05-02 - feat(tower): add projectile attack controller

### Summary
- Added a tower attack controller that fires reusable projectiles at the current tower target.
- Kept attack stats on the tower instance for per-tower damage, cooldown, projectile speed, lifetime, and target layer tuning.
- Added a lightweight fire event for future presentation hooks.

### New or Updated Tests
**EditMode**
- `TowerAttackControllerTests` - verifies no-target behavior, valid target firing, cooldown gating, and projectile launch values.

**PlayMode**
- N/A - N/A

### Notes
- EditMode passed per user in Unity.

## 2026-05-02 - feat(projectile): add reusable projectile runtime

### Summary
- Added a source-agnostic projectile runtime and launch context.
- Created the `Projectile_Arrow` prefab from the imported arrow sprite.
- Added a focused prefab contract test for the reusable projectile setup.
- Updated the arrow projectile contract with a kinematic Rigidbody2D and polygon trigger collider.

### New or Updated Tests
**EditMode**
- `ProjectileArrowPrefabContractTests` - verifies the arrow projectile prefab has runtime, polygon trigger collider, kinematic Rigidbody2D, and sprite wiring.

**PlayMode**
- N/A - N/A

### Notes
- EditMode passed per user in Unity.

## 2026-05-02 - feat(tower): return aim to idle

### Summary
- Added optional idle aim return so towers rotate back to their forward rest angle when no valid target exists.
- Wired the base Tower prefab to return to forward aim when its target is lost, destroyed, or no longer valid.
- Kept idle return visual-only and bypassed whenever a valid current target exists.

### New or Updated Tests
**EditMode**
- `TowerAimControllerTests` - verifies idle return, disabled idle return, destroyed-target return, and valid-target priority over idle return.
- `TowerRuntimeContractTests` - verifies the Tower prefab serializes idle aim return settings for the base arrow tower.

**PlayMode**
- N/A - N/A

### Notes
- EditMode and PlayMode passed per user before commit.

## 2026-05-02 - feat(tower): filter inside castle targets

### Summary
- Updated tower targeting so enemies marked inside the castle region are ignored.
- Kept enemies without region state targetable for simple tests and future non-enemy target dummies.
- Added regression coverage for selecting the next outside enemy and clearing a target that enters the castle.

### New or Updated Tests
**EditMode**
- `TowerTargetingControllerTests` - verifies inside-castle enemies are ignored, outside enemies remain targetable, and current targets clear after entering the castle.

**PlayMode**
- N/A - N/A

### Notes
- EditMode and PlayMode passed per user before commit.

## 2026-05-01 - feat(tower): add optional aim tracking

### Summary
- Added an optional tower aim controller that rotates a configured aim pivot toward the acquired target.
- Wired the base Tower prefab as the arrow tower with aiming enabled by default.
- Added coverage for enabled aim, disabled aim, moving target tracking, no-target safety, smooth rotation, and prefab aim wiring.

### New or Updated Tests
**EditMode**
- `TowerAimControllerTests` - verifies aim rotation, disabled aim behavior, no-target safety, and smooth rotate-toward behavior.
- `TowerRuntimeContractTests` - verifies the Tower prefab serializes the base arrow tower aim contract.

**PlayMode**
- N/A - N/A

### Notes
- EditMode and PlayMode passed per user before commit.

## 2026-05-01 - feat(tower): wire targeting prefab contract

### Summary
- Added a base tower targeting profile asset for the default tower prefab.
- Wired `Tower.prefab` with `TowerTargetingController` and the base targeting profile.
- Added prefab contract coverage for targeting profile assignment, range settings, scan interval, nearest selection, and Enemies layer targeting.

### New or Updated Tests
**EditMode**
- `TowerRuntimeContractTests` - verifies the Tower prefab serializes the base targeting controller and profile contract.

**PlayMode**
- N/A - N/A

### Notes
- EditMode and PlayMode passed per user before commit.

## 2026-04-30 - feat(tower): targeting acquisition contract

### Summary
- Added a tower targeting profile contract for prefab-specific min range, max range, scan interval, target layers, and nearest/farthest selection.
- Added a tower targeting controller that acquires targets with a reusable non-alloc 2D physics buffer.
- Covered mortar-style dead zones, oil-style close range, nearest targeting, farthest targeting, and target clearing behavior.

### New or Updated Tests
**EditMode**
- `TowerTargetingControllerTests` - verifies no-target, min/max range filtering, nearest/farthest selection, and current target clearing.

**PlayMode**
- N/A - N/A

### Notes
- EditMode and PlayMode passed per user before commit.

## 2026-04-30 - fix(tower): allow zero-cost builds

### Summary
- Fixed tower build orchestration so `BuildCost` values of `0` skip the spend call instead of being rejected as insufficient gold.
- Kept normal positive-cost spending and rollback behavior unchanged.
- Added regression coverage for successful zero-cost tower builds with no gold.

### New or Updated Tests
**EditMode**
- `TowerBuildControllerTests` - verifies zero-cost tower builds succeed without changing gold.

**PlayMode**
- N/A - N/A

### Notes
- N/A

## 2026-04-29 - test(playmode): tower build vertical slice

### Summary
- Added a MainPrototype PlayMode smoke for the real between-wave upgrade menu tower build path.
- Verified a menu Build action spends gold once, spawns the configured Tower prefab, and assigns the spawned runtime to a barrier tower plot.
- Verified occupied plot rows stay visible but disabled and duplicate build attempts do not spend or spawn again.

### New or Updated Tests
**EditMode**
- N/A - N/A

**PlayMode**
- `TowerBuildUpgradeMenuVerticalSlicePlayTests` - verifies the integrated barrier plot tower build path through the upgrade menu and duplicate purchase rejection.

### Notes
- N/A

## 2026-04-29 - feat(ui): barrier tower build rows

### Summary
- Extended the between-wave upgrade list to render barrier-owned tower plot child rows beneath each barrier upgrade row.
- Kept tower build spending and spawning delegated to `TowerBuildController` while the UI only renders plot state and invokes actions.
- Added occupied plot presentation so built tower slots stay visible but cannot be repurchased.
- Wired `MainPrototype` to a scene-level `TowerBuildController` and `TowerBuildConfig` so tower rows appear in normal gameplay.
- Tightened tower row text and indentation so child plot rows fit inside the existing menu and read as subordinate barrier actions.
- Replaced shorthand currency copy with game-facing `Gold` labels.
- Enlarged `UpgradeMenuPanel` in `MainPrototype` so the barrier-owned tower rows fit in the authored menu.
- Widened row columns and added barrier-group spacing so buttons do not cover text and each barrier block reads separately.
- Increased the gap before row buttons and between barrier groups for clearer menu scanning.

### New or Updated Tests
**EditMode**
- `UpgradeMenuListViewTowerRowsTests` - verifies barrier upgrade rows, tower child rows, build invocation, occupied plot disabling, existing barrier upgrade action behavior, and MainPrototype tower build wiring.

**PlayMode**
- N/A - N/A

### Notes
- N/A

## 2026-04-29 - feat(gameplay): tower build orchestration

### Summary
- Added a `TowerBuildConfig` asset contract for tower prefab, build cost, and visible future stat fields.
- Added a focused `TowerBuildController` that gates builds to pre-wave, validates plot availability, spends gold once, spawns the configured tower, and assigns it to the plot.
- Reused upgrade success/denied feedback cues so the existing menu flash colors can support tower build outcomes.

### New or Updated Tests
**EditMode**
- `TowerBuildControllerTests` - verifies successful build, invalid prerequisite rejection, occupied plot rejection, insufficient gold rejection, feedback reuse, and config value clamping.

**PlayMode**
- N/A - N/A

### Notes
- N/A

## 2026-04-29 - feat(castle): authored barrier flank tower plots

### Summary
- Authored two reusable tower plot anchors on the barrier prefab so generated barriers inherit left/right flank slots automatically.
- Kept tower plot ownership on `BarrierTowerPlotCollection` while letting `SystemsRoot` rotation carry plot orientation per barrier side.
- Added prefab and generated-barrier integration coverage for two-slot plot wiring and lattice-aligned plot anchors.

### New or Updated Tests
**EditMode**
- `BarrierPrefabVisualContractTests` - verifies the barrier prefab exposes two distinct flank plots under `SystemsRoot`.
- `MainPrototypeBarrierAssemblyIntegrationTests` - verifies generated barriers inherit two linked tower plots with lattice-aligned anchors.

**PlayMode**
- N/A - N/A

### Notes
- Future handoff note: a tile-driven plot-generation pipeline may be added later, but this commit intentionally authors the first flank slots on the barrier prefab itself.

## 2026-04-27 - feat(castle): tower plot contract

### Summary
- Added a reusable `TowerPlot` contract that owns plot anchor and occupancy state without taking on build or UI responsibilities.
- Added `BarrierTowerPlotCollection` so barriers reference a collection of plots instead of fixed left/right slot fields.
- Added EditMode coverage for empty/occupied plot lifecycle, collection-based barrier linkage, and plug-and-play normalization of null/duplicate plot wiring.

### New or Updated Tests
**EditMode**
- `TowerPlotContractTests` - verifies plot anchor fallback, occupant lifecycle, collection-based barrier linkage, and reusable multi-plot authoring behavior.

**PlayMode**
- N/A - N/A

### Notes
- Step 1 intentionally stops at plot contract/state only; build orchestration and authored plot placement follow in later PR slices.

## 2026-04-14 - feat(tower): prefab asset contract coverage

### Summary
- Added stronger tower shell regression coverage around the real prefab asset rather than only synthetic runtime construction.
- Locked the serialized `TowerRuntime` child bindings to the approved `AimPivot`, `TowerVisual`, and `PlatformVisual` nodes.
- Added a PlayMode smoke that instantiates the actual `Tower.prefab` and verifies the runtime shell contract survives asset instantiation.

### New or Updated Tests
**EditMode**
- `TowerRuntimeContractTests` - verifies `TowerRuntime` serialized references bind to the intended prefab children.

**PlayMode**
- `TowerSpawnInitPlayTests` - instantiates the real `Tower.prefab` and verifies root collider/runtime plus hierarchy reference contract.

### Notes
- Manual validation still recommended in the Unity editor before merging `#158`.

## 2026-03-25 - feat(tower): prefab + core runtime

### Summary
- Added a minimal `TowerRuntime` that owns tower core state only: health bounds plus cached hierarchy references for future tower systems.
- Introduced `Tower.prefab` with the locked stationary-root contract: root collider/runtime, `AimPivot`, `TowerVisual`, and `PlatformVisual`.
- Added EditMode and PlayMode tower contract coverage while keeping targeting, firing, projectiles, and build integration out of this PR.

### New or Updated Tests
**EditMode**
- `TowerRuntimeContractTests` - verifies `Tower.prefab` exists, stays normalized, includes runtime/collider state, and preserves the approved child hierarchy contract.

**PlayMode**
- `TowerSpawnInitPlayTests` - verifies a spawned tower initializes runtime state and resolves the required hierarchy references at runtime.

### Notes
- EditMode suite passed via `ci/run-editmode.ps1`. Tower PlayMode coverage was validated manually in the Unity editor after fixing runtime reference resolution for dynamic construction order.

## 2026-03-18 - feat(combat): high-rate cadence contracts (#148/#149/#150)

### Summary
- Moved attack cadence authority into `PlayerAttackLoop` so `PlayerController` remains orchestration-focused.
- Preserved hit delivery for compressed high-rate swings and fixed the same-swing multi-hit regression on overlapping enemies.
- Removed stale mobile attack-rate wiring so Android input now forwards held intent only while combat runtime owns cadence.

### New or Updated Tests
**EditMode**
- `PlayerAttackAuthorityOwnershipContractsTests` - verifies attack cooldown authority moved out of `PlayerController` and into `PlayerAttackLoop`.
- `PlayerAttackCadenceClockContractsTests` - verifies loop-owned cadence uses internal elapsed time instead of controller time sources.
- `PlayerHoldFireContractsTests` - verifies `PlayerController` keeps held-fire intent delegated through `PlayerFireInputController` and into `PlayerAttackLoop`.
- `PlayerAttackLoopTimingTests` - verifies chained swing timing, release behavior, and no-idle chaining across sustained held fire.
- `MobileInputDriverOwnershipContractsTests` - verifies `MobileInputDriver` no longer exposes attack-rate APIs and `PlayerController` no longer syncs cadence into mobile input.

**PlayMode**
- `PlayerHighRateCadenceConsistencyPlayTests` - verifies stable loop cadence across sustained high-rate attack windows.
- `PlayerAttackDamagePlayTests` - verifies compressed high-rate swings still deal repeated damage across swings without allowing same-swing multi-hit overlap.
- `PlayerAttackLoopIdleFlickerPlayTests` - verifies held attack presentation does not pulse to idle between chained swings.

### Notes
- Manual validation confirmed PC and Android hold-fire paths still function, high-rate attacks remain responsive, and unarmed attacks return to one damage per swing.

## 2026-03-06 - feat(input): movement-first facing with conditional aim override (#153)

### Summary
- Added a dedicated `PlayerFacingPolicyResolver` to enforce movement-first facing with explicit aim override intent.
- Kept `PlayerController` orchestration thin by delegating facing-source selection through policy resolution.
- Added EditMode and PlayMode coverage for PC/Android source switching, hysteresis thresholds, and no-movement facing fallback.

### New or Updated Tests
**EditMode**
- `PlayerFacingPolicyContractsTests` - verifies delegated facing-policy contract, tunable thresholds, and source-selection boundaries.
- `PlayerFacingPolicyResolverTests` - verifies PC aim-intent override, Android right-stick hysteresis enter/hold/exit, and no-movement facing preservation.

**PlayMode**
- `PlayerFacingPolicyPlayTests` - verifies runtime movement-facing <-> aim-facing switching for PC and Android policy paths.

### Notes
- Hold-fire cadence and editor touch-zone mouse-bleed regression suites remained green during facing-policy validation.

## 2026-03-06 - feat(input): pc mouse aim + hold-fire cadence (#140/#141)

### Summary
- Implemented PC mouse look binding with world-space aim resolution and left-click attack flow while preserving existing cooldown-gated attack authority.
- Extracted fire/aim responsibilities from `PlayerController` into `PlayerFireInputController` and `PlayerAimInputResolver`, then wired components on `Player.prefab`.
- Fixed editor touchzone bleed so mouse no longer drives virtual mobile sticks in normal PC play, while keeping simulator/touch behavior functional.

### New or Updated Tests
**EditMode**
- `PcMouseAimContractsTests` - verifies mouse look binding and delegated aim resolver contract.
- `PlayerHoldFireContractsTests` - verifies `PlayerController` delegates hold-fire lifecycle to `PlayerFireInputController`.
- `PlayerFireInputControllerTests` - verifies hold-state transitions, tick attack attempts, and release fallback clear behavior.
- `TouchMovementZoneTests` / `TouchAimAttackZoneTests` - verifies runtime handlers ignore mouse pointer events.
- `MobileInputDriverEditorGatingContractsTests` - verifies editor gating contract for simulator touch enable behavior.

**PlayMode**
- `PlayerHoldFireCadencePlayTests` - verifies held fire repeats attack attempts across frames and stops after release.

### Notes
- Manual validation confirmed PC left-click hold starts repeated attacks and release stops immediately.

## 2026-03-05 - feat(ui): death-screen tappable restart button (#139)

### Summary
- Replaced spacebar-only death prompt with touch-friendly restart messaging on the game-over screen.
- Added `GameOverScreenController` so death-screen UI (restart button + label) is owned by canvas UI, while `GameManager` keeps restart flow/state orchestration.
- Preserved keyboard restart behavior by supporting `Space` via both Input System and legacy input fallback.

### New or Updated Tests
**EditMode**
- `GameOverRestartContractsTests` - verifies shared UI restart entrypoint contract and touch-friendly death-screen copy in scenes.
- `GameManagerKeyboardRestartContractsTests` - verifies `Space` restart code path remains present for Input System and legacy input.

**PlayMode**
- `DeathScreenRestartPlayTests` - verifies game-over UI appears on death and includes a tappable restart button.

### Notes
- Manual validation confirmed restart button works on death screen and `Space` restart works on PC after input-backend fallback update.

## 2026-03-05 - refactor/143-split-player-controller (#145 attack speed wiring)

### Summary
- Completed `#143` structural split: extracted `RepairSensor`, extracted `WeaponSlotSwapHandler`, and reduced `PlayerController` to orchestrator responsibilities.
- Implemented `#145` attack-speed wiring: effective rate uses `baseAttackRate * weaponAttackSpeed` and gates real attack cadence (not animation-only speedup).
- Synced mobile attack repeat rate through `MobileInputDriver.SetAttackRate` and tuned attack return timing so hitbox event windows execute reliably.

### New or Updated Tests
**EditMode**
- `MobileInputDriverTests` - `SetAttackRate(float)` API contract and positive-minimum clamping
- `PlayerAttackRateCalculatorTests` - effective rate math and weapon speed change coverage
- `PlayerAttackCooldownGateTests` - cooldown consume cadence and reset behavior
- `AnimatorAttackTransitionContractTests` - attack transition contract (`hasExitTime=true` with short blend duration)
- `PlayerAttackInputTests` / `PlayerWeaponSlotSwapInputTests` - regression guards remained green through the `#143` split

**PlayMode**
- `PlayerAttackDamagePlayTests` - attack animation/hitbox path damages enemies on first and consecutive swings (regression guard for no-damage attack states)

### Notes
- Input debugging note: Device Simulator state caused repeated false negatives for mouse validation during this branch; reliable verification required separate passes (Simulator closed for mouse, open for touch).
- During investigation, several temporary input/debug experiments were intentionally rolled back; final branch state keeps only validated behavior changes.
- Regression note: a short-blend + early normalized attack exit configuration suppressed hitbox damage; guarded by animator contract and PlayMode damage tests.
- Manual validation confirmed PC and Android attack paths both work with reasonable attack-speed values.
- Follow-up enhancements (mouse hold auto-swing and extreme-rate support) deferred to separate issues.

## 2026-03-02 - feat/android-touch-controls

### Summary
- Fixed virtual gamepad device pairing via `InputUser.PerformPairingWithDevice` so
  `PlayerInput` receives input on Android where no physical keyboard exists.
- Disabled `RaycastTarget` on `PlayerHitFlash` full-screen Image, unblocking pointer
  events to `TouchMovementZone` and `TouchAimAttackZone`.
- Fixed `TouchMovementZone.SimulateDrag` analog math: replaced `delta.normalized`
  (always magnitude 1) with `delta / maxRadius` for proportional 0→1 response.
- Added attack repeat: `MobileInputDriver` pulses `rightTrigger` 0→1→0 at
  `baseAttackRate` (1.5/s); `PlayerController.OnFire` guards the release event
  with `value.isPressed`.

### New or Updated Tests
**EditMode**
- `TouchMovementZoneTests` - proportional magnitude at half-radius, full-radius,
  and clamped-beyond-radius
- `MobileInputDriverTests` - null-safety: OnEnable/OnDisable without refs, repeated
  enable/disable cycles; PlayMode gap documented for pulse timer
- `PlayerAttackInputTests` - PlayerController instantiation, input-lock blocks
  movement, unlock restores movement

**PlayMode**
- N/A - device pairing, attack pulse, and isPressed guard require full PlayerInput
  runtime; manual Device Simulator validation performed: move, attack, repair,
  weapon swap, and upgrade panel all confirmed working

### Notes
- Real Android sideload confirmed: move, attack, repair, weapon swap all functional
  on physical device.
- `attackSpeed` stat flows `WeaponDefinition` → `WeaponStats` → `PlayerWeaponController`
  but is not yet consumed; weapon speed scaling deferred to PlayerController refactor.

---

## 2026-02-28 - feat/ci-android-apk-v2

### Summary
- Established Android CI build pipeline via GitHub Actions (workflow, PowerShell runner, `AndroidCiBuildRunner`, `AndroidArchitectureInitializer`, `AndroidBuildPreprocessor`).
- Switched scripting backend to IL2CPP and target architecture to ARM64, resolving "Target architecture not specified" build failure caused by Unity batchmode serialization bug overwriting the architecture artifact during `PrepareForBuild`.
- Fixed Active Input Handling from Both → Input System Package (New), removing unsupported-on-Android warning.

### New or Updated Tests
**EditMode**
- N/A

**PlayMode**
- N/A

### Notes
- Manual validation: APK built successfully via CI pipeline and confirmed loading on a physical Android device.
- Existing EditMode and PlayMode CI pipelines validated as unaffected by IL2CPP/ARM64/input handling changes.

---

## 2026-02-21 - feat/130-barrier-lattice-static-castle

### Summary
- Implemented tilemap-driven barrier assembly pipeline with side-mapped visuals, SystemsRoot rotation contract, and root-stable placement.
- Enforced castle scene collider contracts: wall tilemap blocks, floor tilemap is trigger-region source, and barrier marker tilemap is non-blocking.
- Added runtime and regression coverage for assembly behavior, prefab contracts, lattice/occupancy rules, and localized barrier hit shake.
- Fixed bootstrap clear/rebuild race in PlayMode by detaching generated children before deferred destroy.

### New or Updated Tests
**EditMode**
- ScaleCompensationGuardTests - removed legacy barrier root scale exception and kept normalized scale guard.
- BarrierPrefabNormalizationTests - verifies barrier scale/collider normalization baseline.
- CastleModulePlacementRulesTests - validates 3-unit lattice and occupancy rule decisions.
- BarrierAssemblyBuilderTests - validates deterministic rebuild behavior and generated-child lifecycle.
- BarrierAssemblyRunnerTests - validates runner orchestration from layout source to generated instances.
- BarrierTilemapLayoutSourceTests - validates marker extraction and side mapping from barrier tilemap.
- BarrierVisualBindingTests - validates side sprite binding and SystemsRoot rotation behavior.
- BarrierPrefabVisualContractTests - validates side sprites and SystemsRoot ownership of directional/UI children.
- BarrierPlacementPresentationContractTests - validates marker alignment, marker renderer hiding, and root rotation contract.
- MainPrototypeBarrierAssemblyIntegrationTests - validates scene bootstrap wiring, generation, slot alignment, and side-rotation contract.
- CastleWallTilemapColliderContractTests - validates walls as blocking colliders and barrier marker tilemap as non-blocking.
- CastleFloorRegionContractTests - validates floor trigger collider and CastleRegionTracker region-source contract.
- BarrierHitShakeTests - validates shake localizes to impacted barrier and baseline restore behavior.

**PlayMode**
- CastleTilemapRuntimeContractsPlayTests - validates walls/floor runtime collider contracts, barrier break collider disable behavior, and bootstrap rebuild pipeline with generated barrier rotation/sprite/runtime component contracts.

### Notes
- Manual validation: barriers spawn from marker tilemap, remain in place, and use side-correct visuals with SystemsRoot-driven directional components.
- Manual validation: wall blocks movement, intact barriers block passage, and broken barriers allow player/enemy passage through openings.

## 2026-02-14 - feat-import-castle-tileset-baseline-ppu32

### Summary
- Imported castle tileset baseline assets (walls, corners, floor, barrier set) under `Castle_Assets/Tileset` with palette/tile assets for scene authoring.
- Added castle tileset import contract coverage and scoped enforcement to the tileset folder.
- Applied visual scene layout pass using separate castle floor/walls tilemaps and retuned enemy pass-through radius for the updated gate/opening geometry.

### New or Updated Tests
**EditMode**
- `CastleTilesetImportBaselineTests` - tileset import contract for PPU32, point filter, uncompressed textures, mipmaps off (sprites), and center pivot consistency

**PlayMode**
- N/A - N/A

### Notes
- Manual: verified castle tiles render correctly in `MainPrototype`, floor/walls layering is readable, and enemies can pass into castle after barrier break with updated pass-through tuning.

## 2026-02-14 - feat-scale-baseline-ppu32

### Summary
- Established P3 scale foundation with `MainPrototype` world grid baseline (`Grid` cell size `1x1`) and camera baseline guard (`orthographicSize` range `9..12`).
- Enforced scoped PPU/import defaults across world + UI art (`PPU=32` for sprites, `Point` filtering, uncompressed textures, mipmaps off for sprites).
- Retuned scene/prefab geometry and scale fallout for PPU32 migration, including temporary barrier scale exception guard and pulse ring thickness tuning.

### New or Updated Tests
**EditMode**
- `CameraScaleBaselineTests` - `MainPrototype` orthographic camera baseline contract for PPU32 readability
- `WorldGridBaselineTests` - `MainPrototype` authoritative `Grid` presence and `1x1` cell size
- `PpuImportBaselineTests` - scoped importer contract for PPU/filter/compression/mipmap settings
- `ScaleCompensationGuardTests` - prefab/root/sprite scale guard with explicit temporary barrier exception

**PlayMode**
- `ScaleBaselineSmokePlayTests` - `MainPrototype` load + camera/player visibility smoke at baseline scale

### Notes
- Manual: validated player/items readability at new baseline and retuned barrier/enemy/player scene behavior after PPU32 conversion.

## 2026-02-13 - feat-visual-subtle-castle-pulse-cue

### Summary
- Added ring-only pulse VFX controller driven by barrier pulse runtime state (radius/progress/duration).
- Synced visual origin to `PulseOrigin` so VFX center and gameplay push origin match.
- Added flipbook strip animation + seam drift controls and wired pulse cue sprite/prefab scene defaults.

### New or Updated Tests
**EditMode**
- `BarrierPulseEmitterVisualStateTests` - exposes and updates pulse visual state for VFX followers
- `BarrierPulseVfxControllerTests` - ring follows active pulse radius/window and uses `PulseOrigin` center
- `BarrierPulseVfxControllerConfigTests` - ring config contract (alpha/sorting/tile mode/ring-only controls)
- `BarrierPulseRingFlipbookTests` - flipbook controls, frame advance/loop, and seam drift behavior

**PlayMode**
- `BarrierPulseVfxPlayTests` - cue activation/teardown, runtime radius tracking, flipbook advance, and third-break flow with VFX attached

### Notes
- Manual: validated in `MainPrototype` that pulse cue triggers reliably, uses `PulseOrigin`, and current ring behavior is tuned/acceptable.

## 2026-02-08 - feat-castle-pushback-pulse

### Summary
- Added pulse gating by barrier-side threshold so enemies already past the barrier are not pushed.
- Added wave-index propagation from spawner to provider so pressure trigger resets and re-fires per wave.
- Added loop-count pulse behavior and prefab-tuned defaults for duration/radius/threshold.

### New or Updated Tests
**EditMode**
- `BarrierPulseEmitterTests` - continuous wavefront pressure, loop count active-window behavior, and threshold gating
- `BarrierPressureTrackerTests` - scene-provider fallback and reset on wave changes
- `EnemySpawnerTests` - runner updates `WaveIndexProviderComponent` on wave start
- `BarrierSideClassifierTests` - barrier-side threshold classification contract
- `EnemyControllerKnockbackTests` - knockback still applies when target is null

**PlayMode**
- N/A - N/A

### Notes
- Manual: verified third barrier break triggers pulse, pulse can be loop-configured, and trigger fires again in later waves.

## 2026-02-04 - refactor-castle-region-events

### Summary
- Replaced enemy inside/outside polling with event-driven EnemyRegionState caching.
- CastleRegionTracker now raises enter/exit events and instance-ready signal.
- Enemy prefab updated to include EnemyRegionState; tests cover region state + PlayMode crossing.

### New or Updated Tests
**EditMode**
- CastleRegionTrackerEventsTests - tracker enter/exit events
- EnemyRegionStateTests - cached state updates and missing-tracker fallback
- EnemyControllerRegionStateTests - target selection uses cached state
- EnemyAttackRegionStateTests - attack gating reads cached state
- EnemyAttackRegionTrackerFallbackTests - warning when region state missing

**PlayMode**
- EnemyRegionStatePlayTests - crossing region trigger updates inside state

### Notes
- Manual PlayMode verified enemy behavior.

## 2026-02-01 - feat/castle-pressure-trigger

### Summary
- Added per-barrier break pressure trigger core with wave-aware reset.
- Wired barrier break events to pressure tracking and scene wave index provider.
- Added EditMode tests for trigger rules and barrier break event regression.

### New or Updated Tests
**EditMode**
- `BarrierBreakPressureTriggerTests` - third-break trigger and wave reset behavior
- `BarrierPressureTrackerTests` - per-barrier trigger and wave change reset
- `BarrierHealthTests` - OnBroken event fires once per break

**PlayMode**
- N/A - N/A

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
