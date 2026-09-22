# Buckl web

React + Vite + TypeScript front end of Buckl, packaged as a PWA.

Run from the repository root with the workspace scripts (`npm run dev -w web`, `npm run test -w
web`) or from this directory with `npm run dev`, `npm run test`, `npm run build`.

Design and architecture live in [`docs/`](../../docs/README.md).

The folder layout, routing and data access are described in
[`docs/architecture/web-app.md`](../../docs/architecture/web-app.md).

## Trying the installed app

Build and serve the production bundle with `npm run build -w web && npm run preview -w web -- --host`.
Installation needs a secure origin: on an Android phone connected over USB, run
`adb reverse tcp:4173 tcp:4173` and open `http://localhost:4173` in Chrome, then choose
"Install app". Regenerate the icons after changing `public/icon.svg` with `npm run pwa:assets -w web`.
