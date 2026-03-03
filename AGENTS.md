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
`LOCKED: Yes` or `Approved`

No code before approval.

---

### IMPLEMENTATION MODE (Only after LOCKED / Approved)
- Perform exactly ONE logical unit of behavior per step.
- Scope is defined by the Design Snapshot, not a file count.
- Keep changes small, reversible, and independently testable.
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
- EditMode: `Assets/_Project/_Tests/EditMode/<Feature>/...Tests.cs`
- PlayMode: `Assets/_Project/_Tests/PlayMode/<Feature>/...PlayTests.cs`

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

---

## GitHub Issue Creation (When Requested)

### Rules
- Output ONE issue body in ONE fenced code block.
- Ready to paste into GitHub.
- Do NOT invent issue or PR numbers.
- Use TBD or N/A instead of leaving blanks.
- Infer milestone from current active work context.
- Infer labels from CONTRIBUTING.md label definitions.
- If milestone or labels are ambiguous, ask one clarifying question first.

### Labels (Use Exactly As Listed)

| Label | Usage |
|-------|-------|
| `ai` | Enemy movement, targeting, behavior trees |
| `android` | Android-specific work: builds, packaging, device testing, platform setup |
| `apk` | Opt-in signal to generate an Android APK build for CI testing |
| `art` | Sprites, animations, visual assets |
| `bug` | Something isn't working |
| `ci` | Continuous integration: automated builds, checks, pipelines |
| `docs` | Updates to documentation or guides |
| `feature` | New feature or request |
| `gameplay` | Anything affecting the core loop (combat, waves, health) |
| `infra` | Project infrastructure: build systems, runners, tooling, environment setup |
| `prefab` | Adding or updating prefabs |
| `release` | Work related to milestone, demo, or versioned release builds |
| `scene` | Scene updates, environment layout, prefab arrangement |
| `tech-debt` | Cleanup, refactor, or restructuring of code/assets |
| `testing` | Work specifically related to adding or fixing test setups |
| `ui` | UI menus, HUD, game over screen changes |
| `workflow` | Workflow-specific automation or process-related changes |

Multiple labels are allowed. Always apply the most specific combination that fits.

---

### Issue Title Format (Required)
All issue titles must follow the Conventional Commits naming format:

```
type(scope): short description
```

Valid types: `feat`, `fix`, `chore`, `docs`, `test`, `refactor`, `perf`, `style`
Scope = the system or area touched (e.g. `player`, `ui`, `wall`, `visual`, `gameplay`, `input`, `world`, `balance`)

Examples:
- `feat(inventory): implement castle vault count-based storage`
- `fix(gameplay): wall refund not triggering on removal`
- `refactor(player): split PlayerController responsibilities`
- `chore(docs): update LIVING_ISSUE_TREE after P3 close`

### Template (copy exactly)
```md
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
```

---

## PR Close-Out (Only on Trigger)

When the user says:
`READY TO COMMIT: <summary>`

Output in order:
1. Branch name suggestion
2. Commit message suggestion(s)
3. PR template (below)
4. Suggested labels
5. Squash merge message
6. Cleanup checklist

Do not include Teaching Mode unless explicitly requested.

### Commit Message Format (Required)
All commit messages must follow Conventional Commits format:

```
type(scope): short description
```

- Same types and scopes as issue titles: `feat`, `fix`, `chore`, `docs`, `test`, `refactor`, `perf`, `style`
- Keep the description under 72 characters
- No period at the end
- No Claude co-author line — ever
- No "Generated by" or AI attribution of any kind

Examples:
- `feat(inventory): add castle vault count-based storage`
- `fix(gameplay): wall refund not triggering on removal`
- `chore(docs): update LIVING_ISSUE_TREE after P3 close`

### Squash Merge Message Format (Required)
When outputting the squash merge message, use this format exactly:

```
type(scope): short description (#PR_NUMBER)
```

- Match the type and scope of the primary commit
- Append the PR number in parentheses
- No co-author lines
- No AI attribution of any kind

Example:
- `feat(inventory): add castle vault count-based storage (#47)`

### Pull Request Template (copy exactly)
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
_All items must be checked. If not applicable, check and mark "N/A"._

- [ ] Unit tests (EditMode) added or updated
- [ ] PlayMode test or manual validation included
- [ ] Demo scene updated (if player-visible)
- [ ] Prefab links, tags, and layers validated
- [ ] README / Docs touched (if applicable)

## Related
- Closes #<issue> or Refs #<issue> (if applicable)
```

---

## Context Threshold Protocol

Monitor session length actively. When the session is running long — many exchanges, large code blocks, or complex multi-step work — proactively warn before context degrades:

```
⚠️ CONTEXT WARNING
This session is approaching its reliable limit.
Consider starting a new chat soon.
Type HANDOFF when ready for a clean transition block.
```

Warn early rather than late. It is better to suggest a handoff before it is needed than after quality has already degraded.

---

## HANDOFF Task (Trigger: "HANDOFF")

When the user types `HANDOFF`, immediately output the following block — no commentary, no preamble:

```md
## HANDOFF BLOCK

**Date:** <YYYY-MM-DD>
**Branch:** <current branch>
**Active Issue:** <issue # and title>
**Milestone:** <current milestone and % complete>

**Last completed step:**
<exactly what just finished — be specific>

**Next step:**
<exactly what to do next — be specific>

**Assumptions made this session:**
- <any labeled assumptions Claude made during implementation>
- <decisions made that the next session must honor>

**Relevant files touched:**
- <file path> — <what changed>

**Tests status:**
- EditMode: <passing / failing / not run>
- PlayMode: <passing / failing / not run>

**Paste into new chat:**
Attach AGENTS.md and this block. Say: "Continuing from handoff."
```

---

## Sync Issue Tree Task (Trigger: "sync issue tree")

When the user types `sync issue tree`:
1. Navigate to https://github.com/f1cklepickle/Castlebound/milestones
2. Read current open/closed counts and completion % per milestone
3. Navigate to https://github.com/f1cklepickle/Castlebound/issues for open issue titles
4. Update Docs/LIVING_ISSUE_TREE.md in the local repo to reflect current state
5. Update the Last synced date at the top of the file
6. Output only the changed sections for review before writing

---

## Log Issue Task (Trigger: "log issue: [title]")

When the user types `log issue: [title]`, immediately produce a structured GitHub issue using the GitHub Issue Template defined above.

- Title MUST follow the format: `type(scope): short description`
- Infer milestone from current active work context
- Infer appropriate labels from the Labels table defined above in this file
- Include any relevant file references or issue links from the current session
- Output ONE issue in ONE fenced code block, ready to paste or create
- If milestone or labels are ambiguous, ask one clarifying question before outputting

---

## Task Refresh (Trigger: "task refresh")

When the user types `task refresh`, output this list — clean and scannable, no extra commentary:

- `HANDOFF` — structured transition block for switching sessions
- `sync issue tree` — scan GitHub, update Docs/LIVING_ISSUE_TREE.md in repo
- `log issue: [title]` — create structured GitHub issue mid-session
- `READY TO COMMIT: [summary]` — full PR close-out output
- `task refresh` — this list

---

## Core Philosophy (Always On)
- Structure before code.
- Small components over bloated scripts.
- Prevent large refactors through early design clarity.
- Favor clarity, safety, and intent over cleverness.
