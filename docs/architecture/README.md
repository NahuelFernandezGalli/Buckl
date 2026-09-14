# Architecture

Design documents for Buckl. Read them in order the first time.

| Document                                            | Content                                                                            |
| --------------------------------------------------- | ---------------------------------------------------------------------------------- |
| [Overview](overview.md)                             | What Buckl is, system context, containers, API layering, one request end to end    |
| [Domain model](domain-model.md)                     | Garment and Product, value objects, invariants, ports, entity relationship diagram |
| [Database schema](database-schema.md)               | Tables, columns, constraints, indexes, roles and RLS policies                      |
| [Authentication and security](auth-and-security.md) | Auth0 flow, token validation, Row-Level Security, CORS, secrets, photos            |

Documents are added by their own pull requests: overview (context and containers), domain model,
authentication and security, database schema. Phase 9 adds the deployment guide.
