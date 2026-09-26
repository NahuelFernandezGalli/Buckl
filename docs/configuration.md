# Configuration

How each app gets its settings, locally and when hosted. Nothing that differs per environment and
nothing secret is committed: the repository holds the names, the values that are the same
everywhere, and examples.

## Web app

Vite reads `VITE_` variables when it starts or builds and embeds them in the bundle, so every one
of them is public. Locally they come from `apps/web/.env.local` (copy
[`apps/web/.env.example`](../apps/web/.env.example)); when hosted (phase 9), from the static
host's build settings.

| Variable               | Example                  | Purpose                                                                                  |
| ---------------------- | ------------------------ | ---------------------------------------------------------------------------------------- |
| `VITE_AUTH0_DOMAIN`    | `buckl-dev.us.auth0.com` | Auth0 tenant, without `https://`                                                         |
| `VITE_AUTH0_CLIENT_ID` | `<client id>`            | The "Buckl Web" single-page application                                                  |
| `VITE_AUTH0_AUDIENCE`  | `https://api.buckl.app`  | Identifier of the "Buckl API"; access tokens carry it                                    |
| `VITE_API_BASE_URL`    | `http://localhost:5080`  | Where the API listens; `https://` unless the host is `localhost`, `127.0.0.1` or `[::1]` |

The app checks them when it starts (`src/app/config.ts`) and stops with a message that names every
missing or malformed variable. Tests never read them.

## API

ASP.NET Core reads, in order: `appsettings.json` (defaults, no per-environment values),
`appsettings.<Environment>.json`, user secrets (Development only), and environment variables. In
environment variables, `__` stands for `:` (`Auth0__Domain`).

| Key                           | Secret | Local                                | Hosted (phase 9)          | Purpose                                   |
| ----------------------------- | ------ | ------------------------------------ | ------------------------- | ----------------------------------------- |
| `ConnectionStrings:Buckl`     | yes    | user secrets                         | host secret               | Application role (`buckl_app`) connection |
| `Auth0:Domain`                | no     | user secrets                         | environment variable      | Auth0 tenant, without `https://`          |
| `Auth0:Audience`              | no     | user secrets                         | environment variable      | Identifier of the "Buckl API"             |
| `Cors:AllowedOrigins`         | no     | `appsettings.Development.json`       | `Cors__AllowedOrigins__0` | Browser origins of the web app            |
| `BUCKL_MIGRATIONS_CONNECTION` | yes    | shell variable, only while migrating | CI secret                 | Owner role, for migrations; not the API's |

The API validates the Auth0 and CORS settings when it starts and refuses to start if they are
missing or malformed. [`apps/api/.env.example`](../apps/api/.env.example) lists every key as an
environment variable; the API itself never reads `.env` files. How to set them locally:
[`apps/api/README.md`](../apps/api/README.md).

## Files that stay out of git

The root `.gitignore` ignores `.env` and `.env.*` everywhere except `.env.example`. That covers
`apps/web/.env.local` and `apps/api/src/Buckl.Api/.env`, where the request examples read two access
tokens. gitleaks scans every pull request and GitHub push protection blocks known secret formats;
a leaked secret is rotated, not just deleted.

## Auth0 tenant

One tenant per environment: `buckl-dev` for development; production gets its own in phase 9.

| Item                    | Setting                                                                                                                                                  |
| ----------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------- |
| API "Buckl API"         | Identifier `https://api.buckl.app`, RS256, offline access allowed, access tokens last 3600 s; user-delegated access for all apps, client access for none |
| Machine-to-machine apps | None: client access is "No apps allowed" and the test application Auth0 creates with the API is deleted                                                  |
| Application "Buckl Web" | Single-page application; grant types Authorization Code and Refresh Token                                                                                |
| Callback URLs           | `http://localhost:5173/callback`, `http://localhost:4173/callback`                                                                                       |
| Logout URLs             | `http://localhost:5173/welcome`, `http://localhost:4173/welcome`                                                                                         |
| Web origins             | `http://localhost:5173`, `http://localhost:4173`                                                                                                         |
| Refresh tokens          | Rotation on, reuse interval 0, absolute lifetime 30 days, inactivity lifetime 15 days                                                                    |
| Connection              | `Username-Password-Authentication`, sign-ups disabled, enabled only for "Buckl Web"; no social logins                                                    |
| Users                   | Two test users created by hand; their passwords live in the maintainer's password manager                                                                |

Port 4173 is `vite preview`, used to try the installed app on a phone.
