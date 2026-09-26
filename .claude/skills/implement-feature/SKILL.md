---
name: implement-feature
description: Step-by-step workflow for implementing a non-trivial feature or behavioural change in this repository, from requirement to verified, reviewable change. Use when asked to implement a story, feature or multi-file change.
---

# Implement a feature

Follow these phases in order. The project instructions (`CLAUDE.md`) and the documents and rule files it points to are the source of truth for architecture, conventions, limits and approval rules. This skill only defines the workflow. If they conflict, the project instructions win.

## 1. Understand the requirement

- Identify the issue and its acceptance criteria. If either is missing, ambiguous or contradicts the codebase, ask before continuing.
- Restate the goal in one sentence and list each acceptance criterion you will verify.

## 2. Read the documentation first

- Read the project documentation that the project instructions require for this kind of change, and every scoped rule file that applies to the areas you will touch.
- Do this before reading implementation code, so the code is read against the intended design.

## 3. Identify the affected layers

- Map the change onto the architectural layers described in the documentation.
- Note which dependency rules and conventions apply to each affected layer.

## 4. Inspect only what is relevant

- Read the code on the path of the change and its direct dependencies: the types you will modify, their callers, and the existing tests for them.
- Search for existing callers or consumers of anything whose contract may change.
- Don't browse unrelated areas. If the code contradicts the documentation, trust the code and report the mismatch.

## 5. Plan, then wait for approval

Produce a short plan containing:
- the issue key and a one-line summary
- the approach, and alternatives with their tradeoffs when there is a real choice
- the files to create or modify, with a total count
- the rule files applied
- how each acceptance criterion will be met and tested
- open questions and risks

Don't write code until the developer approves the plan.

## 6. Check the PR size limit

- Compare the file count against the PR file limit in the project instructions. Count every file: tests, DTOs, registrations and generated migration files.
- If the count is over the limit, propose a split before implementing. Each part should be coherent, build on its own and be reviewable on its own. Order the parts by dependency and follow the project's stacked-PR rules.

## 7. Implement the approved scope

- Implement only what was approved, one part of the split at a time.
- Preserve the existing architecture, dependency direction and conventions. Match the style of each file you edit.
- Don't refactor, rename or reformat unrelated code, and don't add abstractions the plan didn't include.
- If you find a bug or gap outside the scope, report it and propose an issue for it. Don't fix it silently.
- If the approved plan turns out to be wrong or incomplete, stop and explain the problem before deviating from it.

## 8. Test

- Add or update tests for every behavioural change, at the level the project's testing guidance prescribes for each layer.
- Build the solution, then run the relevant tests. Record the actual results, including failures. Never claim a test passed without running it.

## 9. Review the diff

- Review the full diff against the base branch.
- Remove unrelated changes, debug code, commented-out code and secrets.
- Confirm the changed-file count is still within the limit.

## 10. Report

Report:
- the files changed, grouped by layer
- the tests added and the tests run, with their real results
- each acceptance criterion marked ✅ met (with how it was verified), ⚠️ partially met or ❌ not met
- anything you didn't verify, remaining risks, and follow-up issues you propose

Don't commit, push or open a PR unless the developer explicitly approves that step.
