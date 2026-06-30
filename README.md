# Castlebound: Siege Eternal

Castlebound: Siege Eternal is a top-down 2D siege survival prototype built in Unity. Defend a castle against enemy waves, repair damaged barriers, build defenses, collect loot, and keep the run alive as pressure escalates.

## Status

- Active prototype development
- Android-first design target
- Current milestone tracking lives in `Docs/LIVING_ISSUE_TREE.md`

## Current Gameplay

- Wave-based castle defense
- Repairable barriers
- Player melee combat
- Buildable defensive structures
- Upgrade and balance systems
- Enemy pressure against castle gates
- Loot and inventory foundations

## Requirements

- Unity `2022.3.62f2`
- Windows recommended for the current local CI scripts

## Run the Game

1. Open the project in Unity `2022.3.62f2`.
2. Open `Assets/_Project/Scenes/MainPrototype.unity`.
3. Press Play.

## Run Tests

From Unity:

- Test Runner -> EditMode -> Run All
- Test Runner -> PlayMode -> Run All

From PowerShell:

```powershell
.\ci\run-editmode.ps1
.\ci\run-playmode.ps1
```

## Project Layout

- `Assets/_Project/Scripts` - gameplay code
- `Assets/_Project/Prefabs` - production prefabs
- `Assets/_Project/Scenes` - playable scenes
- `Assets/_Project/Art` - project art assets
- `Assets/_Project/_Tests` - EditMode and PlayMode tests
- `Docs/LIVING_ISSUE_TREE.md` - milestone and issue snapshot
- `Docs/TEST_LOG.md` - test coverage and validation history
- `AGENTS.md` - Codex workflow and project rules

## Development Workflow

- Work on feature branches.
- Use Conventional Commits.
- Open pull requests into `main`.
- Keep EditMode and PlayMode tests passing.
- Squash merge completed PRs.

## License

Copyright 2025 Harrison Bruhl. All rights reserved.

Personal and educational use permitted. Commercial use by permission only.
