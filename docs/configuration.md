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

| Key                            | Secret | Local                                | Hosted (phase 9)          | Purpose                                                             |
| ------------------------------ | ------ | ------------------------------------ | ------------------------- | ------------------------------------------------------------------- |
| `ConnectionStrings:Buckl`      | yes    | user secrets                         | host secret               | Application role (`buckl_app`) connection                           |
| `Auth0:Domain`                 | no     | user secrets                         | environment variable      | Auth0 tenant, without `https://`                                    |
| `Auth0:Audience`               | no     | user secrets                         | environment variable      | Identifier of the "Buckl API"                                       |
| `Cors:AllowedOrigins`          | no     | `appsettings.Development.json`       | `Cors__AllowedOrigins__0` | Browser origins of the web app                                      |
| `PhotoStorage:ServiceUrl`      | no     | user secrets                         | environment variable      | S3 endpoint of the B2 bucket, `https://s3.<region>.backblazeb2.com` |
| `PhotoStorage:Region`          | no     | user secrets                         | environment variable      | The region in that endpoint, such as `us-east-005`                  |
| `PhotoStorage:Bucket`          | no     | user secrets                         | environment variable      | Bucket of garment photos                                            |
| `PhotoStorage:AccessKeyId`     | no     | user secrets                         | host secret               | keyID of a B2 application key, Read and Write, on that bucket only  |
| `PhotoStorage:SecretAccessKey` | yes    | user secrets                         | host secret               | The application key itself                                          |
| `BUCKL_MIGRATIONS_CONNECTION`  | yes    | shell variable, only while migrating | CI secret                 | Owner role, for migrations; not the API's                           |

The API validates the Auth0, CORS and photo storage settings when it starts and refuses to start
if they are missing or malformed. [`apps/api/.env.example`](../apps/api/.env.example) lists every
key as an environment variable; the API itself never reads `.env` files. How to set them locally:
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

## Photo storage

One Backblaze B2 bucket per environment: `buckl-photos-dev` for development; production gets its
own in phase 9.

| Item            | Setting                                                                                                                                                                                   |
| --------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Bucket type     | Private (`allPrivate`), encrypted at rest (SSE-B2), no object lock. Photos are served only through presigned URLs                                                                         |
| CORS            | One rule, set with the B2 CLI: operation `s3_put` from the web app's origins (`http://localhost:5173`, `http://localhost:4173`), header `content-type`                                    |
| Lifecycle       | Two rules, which must not overlap: `uploads/` hides files after 1 day and deletes them 1 day later; `users/` never hides on its own and deletes hidden files 1 day after they were hidden |
| Application key | Read and Write, limited to the bucket; it lives only in user secrets or the host                                                                                                          |
| Free tier       | 10 GB stored, 1 GB downloaded and 2,500 class B and C transactions a day; no payment method on file                                                                                       |

The CORS rule, for when the bucket is created again (the web console cannot write custom rules):

```bash
b2 bucket update --cors-rules "[{\"corsRuleName\":\"webAppUploads\",\"allowedOrigins\":[\"http://localhost:5173\",\"http://localhost:4173\"],\"allowedOperations\":[\"s3_put\"],\"allowedHeaders\":[\"content-type\"],\"maxAgeSeconds\":3600}]" buckl-photos-dev allPrivate
```
