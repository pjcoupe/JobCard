---
name: writing-plans
description: "Create a concrete, step-by-step implementation plan from an approved design before any coding starts."
disable-model-invocation: true
---

# Writing Plans

Convert an approved design into an actionable implementation plan with ordered tasks, risk checks, and validation steps.

## Hard Gate

Do not write or modify production code while running this skill. This skill only creates the implementation plan.

## Inputs

- Approved design (or spec) from `/brainstorming`
- Current project context (code structure, dependencies, constraints)

## Required Workflow

1. Confirm the approved spec/design file path and restate scope in 3-6 bullets.
2. Inspect relevant project files and identify affected modules.
3. Produce a phased implementation plan with:
   - Task order
   - File-level change list
   - Data/model/API impacts
   - Edge cases and rollback strategy
4. Add a verification plan:
   - Build/test commands
   - Manual validation steps
   - Acceptance criteria mapped to scope
5. Highlight risks, open questions, and assumptions.
6. Ask user approval before any implementation action.

## Output Format

Use this structure:

### Scope
- ...

### Implementation Phases
1. ...
2. ...

### File Changes
- `path/to/file`: reason for change

### Validation Plan
- Command(s): `...`
- Manual checks: `...`

### Risks and Open Questions
- ...

### Approval Gate
- "Approve this plan to begin implementation."
