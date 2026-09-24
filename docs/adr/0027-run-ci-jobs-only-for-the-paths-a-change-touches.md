# ADR-0027: Run CI jobs only for the paths a change touches

- **Status:** Accepted
- **Date:** 2026-09-24
- **Deciders:** NahuelFernandezGalli

## Context

[ADR-0011](0011-gate-merges-with-local-hooks-and-ci.md) runs every check on every pull request. A
documentation change still installs and builds the web app, builds and tests the API and audits
both dependency trees, and an API-only change builds the web app. The audit can also turn a
documentation pull request red when an advisory is published for an unchanged dependency. At the
same time, nothing in CI checks the formatting of Markdown, JSON and YAML files.

## Decision

We run each CI job only when a pull request touches the paths it validates. A `Detect changes` job
with `dorny/paths-filter` decides:

- `CI / Web` runs for `apps/web/**`, the root `package.json` and `package-lock.json`.
- `CI / Api` runs for `apps/api/**`.
- `Security / Dependency audit` runs for the npm manifests and lockfile, `global.json`,
  `dotnet-tools.json`, `Directory.Packages.props`, `Directory.Build.props` and `*.csproj`.
- A change to a workflow file runs every job it defines.

The filtered jobs stay in the workflows and are skipped with `if:`, because a skipped job satisfies
a required check while a workflow filtered with `paths` leaves it pending forever. They always run
on pushes to `develop` and `main`, on the weekly security schedule, and whenever change detection
does not succeed, so a broken filter runs too much rather than too little.

`Security / Secret scan`, `PR title / Conventional Commits` and a new `CI / Format` job, which runs
`npm run format:check`, run on every pull request.

## Alternatives considered

- **`paths-ignore` on the workflow** — required checks never report and block the merge.
- **One aggregate required check** — needs the ruleset changed and adds a job to every run, with no
  benefit while there are three filtered jobs.
- **Keep running everything** — simple, but slow for most pull requests and noisy for audits.

## Consequences

### Positive

- Documentation pull requests finish in the time of the format, secret and title checks.
- Web-only and API-only pull requests skip the other application's build.
- Formatting of documentation and configuration files is enforced on the server.

### Negative

- A new path that affects a job must be added to its filter, or changes to it go unchecked until
  the push to `develop`.
- A new advisory against unchanged manifests shows up on the weekly run, not on the next pull
  request.
