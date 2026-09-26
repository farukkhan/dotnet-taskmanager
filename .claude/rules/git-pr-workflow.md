# PR Workflow

When a Jira issue needs more than one PR (see the 10-file limit in `CLAUDE.md`), stack them:

- PR1 targets master.
- PR2 targets PR1's branch.
- PR3 targets PR2's branch.
- Only the first PR in a stack is Ready for Review.
- Dependent PRs remain Draft.
- Open dependent PRs with `gh pr create --draft` and start the description with "Stacked on #<parent>".
- Merge PRs with a merge commit, not squash, so dependent PRs don't show the parent's changes again.
- After a parent PR merges, update the dependent branch against current master.
- Update the dependent branch by merging `origin/master` into it (no rebase or force-push on pushed branches).
- After a parent PR merges, change the dependent PR's base to master (`gh pr edit <n> --base master`) before marking it Ready for Review.
- Then mark the dependent PR Ready for Review.
- Do not merge a dependent PR before its parent.
- Do not automatically delete branches that are still used as a PR base.
- Delete a stack's branches only after no open PR uses them as a base.
