# Evolvera Design Decision Records (DDRs)

This directory is the version-controlled decision history for Evolvera. Keep one Markdown file per consequential game-design decision, such as a mechanic, system interaction, algorithm, or foundational design constraint.

## Workflow

1. **Discuss:** Explore a decision in ChatGPT or elsewhere; do not treat conversation-only text as authoritative.
2. **Draft:** Create `DDR-0001-short-kebab-title.md` from `DDR-TEMPLATE.md`, with **Status: Proposed**. Include rationale, alternatives, consequences, acceptance criteria, and open questions. Do not invent missing mechanics.
3. **Review:** Compare against `Evolvera_Design_Updated_2026-11-13.md` (or its successor), relevant existing DDRs, and the actual C# implementation. Resolve conflicts explicitly.
4. **Approve:** Only the project owner can change a DDR to **Accepted**. Record the approval date and rationale in its history; a chat suggestion or agent edit is not approval.
5. **Implement:** Claude Code may implement accepted DDRs when authorized. Update implementation mapping, tests, and PR/commit references; don't rewrite design rules silently to match the code.
6. **Maintain:** If a decision changes, normally create a new DDR that **Supersedes** the old one. Mark the old DDR **Superseded** and link to the replacement. Preserve both in Git.
7. **Commit:** Commit the DDR and any approved master-design changes together, or in a separate design-only commit. Push and review through the normal GitHub workflow.

## Status definitions

- **Proposed** — Draft requiring approval. Not implementation authorization.
- **Accepted** — Approved design, authoritative until superseded.
- **Implemented** — Accepted and verified in code; cite the validating tests and commit/PR.
- **Rejected** — Considered and explicitly not adopted.
- **Superseded** — Replaced by a later DDR; retain for history.

## Numbering and file naming

Use sequential stable IDs: `DDR-0001-short-title.md`, `DDR-0002-short-title.md`.
Do not reuse IDs, including rejected or superseded ones. The ID and filename are permanent. See `INDEX.md` for the registry.

## Source-of-truth hierarchy

1. Accepted/Implemented DDRs: approved individual decisions.
2. Master design: coherent current product specification, updated to reflect approved DDRs.
3. Code and tests: actual implemented behavior, which may lag design.
4. Proposed DDRs and chat transcripts: exploratory, not approved.

When an accepted DDR conflicts with the master design, flag the discrepancy and reconcile the master document rather than quietly picking one.

## Agent rules

- Never mark a DDR Accepted, Implemented, Rejected, or Superseded without explicit owner approval or verification appropriate to the status.
- Never fabricate implementation facts, approval, equations, or test results.
- Keep `INDEX.md` current with each DDR's status.
- Include links to relevant DDRs in implementation PRs or commits.
