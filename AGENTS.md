# AGENTS.md — Castlebound: Siege Eternal (Unity 2D Mobile)

> NOTE:
> This is a single authoritative agent file.
> Sections below define workflow rules, design gates, and required templates.
> Templates are included inline to prevent drift.

---

## Project Conventions
- Unity 2D only (Rigidbody2D, Collider2D, Physics2D).
- One class per file.
- PascalCase class names; camelCase variables.
- Use [SerializeField] for inspector tuning; avoid unnecessary public fields.
- Respect existing folder structure and repo organization.

---

## Tags & Layers (Contract)
Tags: Player, Enemy, Wall, Projectile  
Layers: Player, Enemies, Walls, Environment  
Do not introduce new tags/layers without asking.

---

## Two-Mode Workflow (Spec Gate)

### DISCUSSION MODE (Default)
Before ANY code edits, provide a Design Snapshot:

1) Goal (1–2 sentences)
2) Responsibility split (existing vs new components)
3) Contracts (interfaces/events/data)
4) Prefab / Scene contract (components, tags, references)
5) Test plan (new + regression)
6) Risks (bloat, coupling, mitigation)

Then STOP and wait for:
LOCKED: Yes or Approved

No code before approval.

---

### IMPLEMENTATION MODE (Only after LOCKED / Approved)
- Perform exactly ONE small actionable unit.
- Scope:
  - 1 concept OR
  - up to 3 small files
- Keep tests green.
- Show only the changed slice of code by default.

---

## Modular Design Guardrails (Anti-God-Script)
- Prefer composition over expansion.
- Controllers orchestrate; they do not own every responsibility.
- New responsibility ⇒ new component.
- If a file trends large (~250–350+ lines), propose extraction early.
- Avoid tight coupling; prefer interfaces/events.

---

## TDD Rules (Required)
- Red → Green → Refactor.
- ≥1 EditMode test for new logic.
- PlayMode tests for smoke/critical paths.
- If touching Player / Enemy / Wall / Waves / Damage:
  - Add 1 regression guard test.

Test paths:
- EditMode: Assets/_Tests/EditMode/<Feature>/...Tests.cs
- PlayMode: Assets/_Tests/PlayMode/<Feature>/...PlayTests.cs

---

## TEST_LOG.md (Required)

### Rules
- Newest entries first.
- Use the exact structure below.
- Summary = bullets only.
- If no PlayMode tests or Notes, write `- N/A`.
- When updating TEST_LOG.md:
  - Output ONLY the new entry block (never the whole file).

### Template (copy exactly)
```md
## YYYY-MM-DD - <branch-or-topic>

### Summary
- <bullet 1>
- <bullet 2>
- <bullet 3>

### New or Updated Tests
**EditMode**
- <TestClassName> — <behavior covered>

**PlayMode**
- <TestClassName or N/A> — <behavior covered or N/A>

### Notes
- <optional; keep short, or write N/A>
```
GitHub Issue Creation (When Requested)
Rules
Output ONE issue body in ONE fenced code block.

Ready to paste into GitHub.

Do NOT invent issue or PR numbers.

Use TBD or N/A instead of leaving blanks.

Template (copy exactly)
**Why**
- <problem / motivation>

**Proposed Change**
- <high-level approach>
- <key bullets only>

**Acceptance Criteria**
- [ ] <observable outcome #1>
- [ ] <observable outcome #2>

**Testing / Validation**
- Manual:
  - <steps or N/A>
- Automated:
  - EditMode: <test name(s) or N/A>
  - PlayMode: <test name(s) or N/A>

**Context (Optional)**
- Related: <link or N/A>
- Milestone: <name or N/A>
PR Close-Out (Only on Trigger)
When the user says:
READY TO COMMIT: <summary>

Output, in order:

Branch name suggestion

Commit message suggestion(s)

PR template 

---

## Pull Request Template (Required)

When generating PR content, the assistant MUST use the following template
exactly and output it in ONE fenced code block when the user says:

READY TO COMMIT: <summary>

### Rules
- All sections must be present.
- All checklist items must be checked.
- If an item is not applicable, check it and mark “N/A”.
- Do NOT omit sections.
- Do NOT add extra sections.

### Template (copy exactly)
```md
# Title
feat(scope): short summary

## Why
One or two sentences describing the problem, goal, or motivation.

## What changed
- Bullet points of the key changes
- Focus on behavior and structure, not file lists

## How to test
1. Open the relevant scene or demo (if applicable)
2. Press Play → verify expected behavior
3. Run tests:
   - EditMode
   - PlayMode (or note manual validation)

## Checklist
_All items must be checked. If not applicable, check and mark “N/A”._

- [ ] Unit tests (EditMode) added or updated
- [ ] PlayMode test or manual validation included
- [ ] Demo scene updated (if player-visible)
- [ ] Prefab links, tags, and layers validated
- [ ] README / Docs touched (if applicable)

## Related
- Closes #<issue> or Refs #<issue> (if applicable)
```

Suggested labels

Squash merge message

Cleanup checklist

Do not include Teaching Mode unless explicitly requested.

Core Philosophy (Always On)
Structure before code.

Small components over bloated scripts.

Prevent large refactors through early design clarity.

Favor clarity, safety, and intent over cleverness.
