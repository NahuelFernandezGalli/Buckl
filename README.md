# Buckl

Your closet, digitized, upload once, dress smarter.

Buckl is a personal wardrobe app: photograph or import the clothes you own, browse them from your
phone or the browser, and get help deciding what to wear. It is a portfolio project built with
React (PWA) and a .NET API on Postgres.

## Documentation

Everything about the design, the decisions and the vocabulary lives in [`docs/`](docs/README.md).

## Repository layout

- `apps/web` — React + Vite + TypeScript front end (PWA).
- `apps/api` — .NET solution: domain and its tests (phase 2); the remaining layers arrive in phase 4.
- `docs` — architecture, ADRs, glossary, privacy.

## Development

Requires Node 22 or newer and the .NET 10 SDK, pinned by `apps/api/global.json`.

Web:

```bash
npm install
npm run lint
npm run typecheck
npm run test
npm run build
```

API (domain only until phase 4):

```bash
dotnet build apps/api
dotnet test apps/api
dotnet format apps/api --verify-no-changes
```

Commits follow Conventional Commits and are checked by local hooks and CI. Feature branches are
merged into `develop`; `develop` is merged into `main` at the end of each phase.

## License

Released under the [MIT License](LICENSE).
