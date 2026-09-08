# Buckl

Your closet, digitized, upload once, dress smarter.

Buckl is a personal wardrobe app: photograph or import the clothes you own, browse them from your
phone or the browser, and get help deciding what to wear. It is a portfolio project built with
React (PWA) and a .NET API on Postgres.

## Documentation

Everything about the design, the decisions and the vocabulary lives in [`docs/`](docs/README.md).

## Repository layout

- `apps/web` — React + Vite + TypeScript front end (PWA).
- `apps/api` — .NET API (added in a later phase).
- `docs` — architecture, ADRs, glossary, privacy.

## Development

Requires Node 22 or newer.

```bash
npm install
npm run lint
npm run typecheck
npm run test
npm run build
```

Commits follow Conventional Commits and are checked by local hooks and CI.

## License

Released under the [MIT License](LICENSE).
