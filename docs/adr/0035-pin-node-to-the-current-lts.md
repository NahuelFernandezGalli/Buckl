# ADR-0035: Pin Node.js to the current LTS release

- **Status:** Accepted
- **Date:** 2026-10-20
- **Deciders:** NahuelFernandezGalli

## Context

Nothing in the repository said which Node.js version the web app and its tooling need. The README
asked for "Node 22 or newer", CI hard-coded `node-version: 22` in three jobs (and resolved it to
whatever 22.x the runner had), and development machines ran Node 24. Meanwhile the dependencies
moved on their own: jsdom 30, react-router 8.4, lint-staged 17.6 and others now declare
`engines.node` ranges such as `^22.22.2 || ^24.15.0 || >=26.0.0`, so an older 22.x or 24.x install
fails in ways that look unrelated to the version. Phase 9 adds a third place that runs Node, the
static host that builds the web app, and it picks its own default unless told otherwise.

## Decision

The repository targets the current Node.js LTS line, **Node 24**, the same way
[ADR-0014](0014-target-the-current-dotnet-lts.md) does for .NET:

- `.nvmrc` at the root holds the major version (`24`) and is the single source of truth. CI reads
  it with `actions/setup-node`'s `node-version-file`, version managers and most static hosts read
  it too.
- The root `package.json` declares `engines.node: "^24.15.0"`: the lowest 24.x the installed
  dependencies accept. npm warns (`EBADENGINE`) on any other version. Installs are not made strict,
  so automated tooling that runs its own Node, such as Dependabot, keeps working.
- CI's path filters treat `.nvmrc` like a manifest, so changing it runs the web and dependency
  audit jobs.

Moving to the next LTS (Node 26) is a deliberate change: a new ADR, `.nvmrc` and `engines`
updated together, and a green CI run.

## Alternatives considered

- **Stay on Node 22** — still supported until April 2027, but development machines already run 24,
  and pinning the older line would only delay the same change.
- **Pin a full version in `.nvmrc`** (`24.15.0`) — reproducible to the patch, but every security
  release would need a commit, and CI and hosts would miss patches in between.
- **`engine-strict=true` in `.npmrc`** — enforces the range on every install, but breaks any tool
  that installs with a different Node, Dependabot included.
- **Leave it implicit** — the state this ADR replaces: each environment picks its own version.

## Consequences

### Positive

- Local machines, CI and the future static host build with the same Node line.
- A dependency that raises its minimum shows up as an `engines` warning instead of an obscure
  failure.

### Negative

- CI moves from Node 22 to 24; a dependency that misbehaves on 24 would surface here first.
- `.nvmrc` and `engines` must be kept in step when a dependency raises its 24.x minimum.
