© 2025 Harrison Bruhl. All rights reserved.
Personal and educational use permitted. Commercial use by permission only.

---

# Contributing to Castlebound: Siege Eternal

These guidelines describe how to develop and maintain **Castlebound: Siege Eternal**.  
They apply both to the solo-dev workflow and any future collaborators.

---

## 🧭 Branching Rules

- Work only on **feature branches** (never commit directly to `main`).  
- Branch naming:
  - `feat/<feature-name>` → new gameplay or systems
  - `fix/<bug-name>` → bug fixes
  - `chore/<meta-change>` → CI / cleanup / non-game logic
  - `docs/<topic>` → documentation

Example:

```bash
git checkout -b feat/enemy-chase-ai
```

---

## 💬 Commit Message Rules (Conventional Commits)

Format:

```
type(scope): short description
```

Examples:

```
feat(player): add movement input
fix(ci): correct PlayMode workflow path
docs(readme): add setup instructions
```

Types include: `feat`, `fix`, `chore`, `docs`, `test`, `refactor`, `style`, `perf`.

---

## 🔀 Pull Requests

- All changes merge via Pull Request into `main`.
- Use the PR template and fill it out completely:
  - Summary: what and why
  - How to Test: Unity Test Runner + manual steps
  - Checklist: tests / CI / docs / cleanup
- CI must be green before merge.
- Merge style → Squash & Merge only.

Example PR title:

```
feat(wall): add repair interaction system
```

---

## 🧪 Testing Expectations

Before pushing:

- Run all EditMode and PlayMode tests locally via Test Runner.
- Fix any failures before commit.
- Add tests for new logic where possible.

CI:

- GitHub Actions runs both test types on each PR and blocks merges on failure.

---

## 🏷 Labels

| Label      | Usage                              |
| ---------- | ---------------------------------- |
| `feature`  | Gameplay or system additions       |
| `fix`      | Bug fixes                          |
| `docs`     | Documentation updates              |
| `workflow` | CI / automation changes            |
| `testing`  | New or updated tests               |
| `tech-debt`| Refactor or cleanup                |
| `question` | Discussion or clarification        |

---

## 🧱 Code Style & Folder Conventions

C# Style:

- PascalCase for classes
- camelCase for variables
- `[SerializeField]` for private Inspector fields

Folders:

- `_Project/Scripts/`
- `_Project/Prefabs/`
- `_Project/Art/`
- `_Project/Animations/`
- `_Tests/`
- `Scenes/`

---

## 🤝 Solo Dev vs Future Contributors

Currently maintained by a solo developer, but structured for future growth.  
All contributors follow the same workflow to keep merges consistent and testable.

