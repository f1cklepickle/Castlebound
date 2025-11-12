# 🏰 Castlebound: Siege Eternal

**Castlebound: Siege Eternal** is a top-down 2D medieval siege survival game built in Unity.  
Defend your castle from waves of enemies, repair walls, and survive as long as you can.  
Designed for **mobile (Android first)** and developed as a **solo-dev project**.

---

## ⚙️ Project Status

> **Current Phase:** Prototype foundations complete (CI + Tests + Docs)  
> Building toward: first playable loop — movement, attack, enemies, wall repair.

✅ EditMode + PlayMode tests passing locally and in GitHub Actions  
✅ Branch protection and squash-only merges active  
✅ CI automation and self-hosted runners configured

---

## 🧩 Tech Stack

- **Engine:** Unity 2023 LTS (2D)
- **Language:** C#
- **IDE:** Visual Studio Code
- **Platform:** Android (primary), Windows Editor testing
- **Version Control:** Git + GitHub
- **CI:** GitHub Actions (EditMode + PlayMode)

---

## ▶️ How to Run the Game

1. Clone the repository  
   ```bash
   git clone https://github.com/f1cklepickle/Castlebound.git
   ```
2. Open the project in Unity 2023 LTS.
3. Load the MainPrototype scene (`Scenes/MainPrototype.unity`).
4. Press Play in the Unity Editor.

---

## 🧪 How to Run Tests

### In Unity

- Window ▸ General ▸ Test Runner
- Choose:
  - EditMode (logic/unit)
  - PlayMode (scene/behavior)
- Click Run All — all tests should pass.

### In GitHub Actions

- Every Pull Request automatically runs both test categories.
- Merge is blocked if any tests fail.

---

## 🧱 CI / Workflow Overview

- All work happens on feature branches (`feat/...`, `fix/...`, `docs/...`, etc.).
- Commits follow Conventional Commits.
- Pull Requests use a fixed template with “How to Test” and checklist.
- Merges to `main` must:
  - Pass CI
  - Use Squash & Merge
- See [CONTRIBUTING.md](CONTRIBUTING.md) for details.

---

## 📊 CI Status

Workflow  | Status
--------- | ------
EditMode Tests | 
PlayMode Tests | 

---

## 🛠 Future Roadmap (Preview)

- Expand castle construction & upgrades
- Add new enemy types and attack patterns
- Introduce traps, towers, and keep upgrades
- Implement wave scaling and progression
- Optimize for full Android builds

