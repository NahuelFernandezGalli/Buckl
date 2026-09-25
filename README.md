# Buckl

Your closet, digitized, upload once, dress smarter.

Buckl is a personal wardrobe app: photograph or import the clothes you own, browse them from your
phone or the browser, and get help deciding what to wear. It is a portfolio project built with
React (PWA) and a .NET API on Postgres.

## Documentation

Everything about the design, the decisions and the vocabulary lives in [`docs/`](docs/README.md).

## Repository layout

- `apps/web` — React + Vite + TypeScript front end (PWA).
- `apps/api` — .NET 10 solution: domain, application, infrastructure and API projects, with one
  test project per layer.
- `docs` — architecture, ADRs, glossary, privacy.

## Development

Requires Node 22 or newer and the .NET 10 SDK, pinned by `apps/api/global.json`.

API tests need Docker running, because they start Postgres in a container. Everything about the
API, from tests to migrations, is in [`apps/api/README.md`](apps/api/README.md).

To run the API against a database, see "Running the API locally" in the same file.

Web:

```bash
npm install
npm run lint
npm run typecheck
npm run test
npm run build
```

API, from `apps/api` (the SDK reads `apps/api/global.json` from the current directory):

```bash
dotnet build
dotnet test
dotnet format --verify-no-changes
```

Both apps read their settings from outside the repository; see
[`docs/configuration.md`](docs/configuration.md).

Commits follow Conventional Commits and are checked by local hooks and CI. Feature branches are
merged into `develop`; `develop` is merged into `main` at the end of each phase.

## License

Released under the [MIT License](LICENSE).
