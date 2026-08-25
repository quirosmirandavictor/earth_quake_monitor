# Architecture Decisions

This document records the main decisions for Earthquake Monitor and the reasoning behind them.

## ADR-001: Use a Modular Monolith for V1
**Status:** Accepted

The first release uses one backend deployment organized into explicit modules. This keeps deployment, debugging, local development, and operational ownership simple while preserving boundaries that can later support extraction into services if justified.

## ADR-002: Apply Clean Architecture
**Status:** Accepted

Domain and use cases do not depend on Azure Functions, Oracle, USGS, or framework-specific details. Dependencies point inward toward business rules.

## ADR-003: Use React and TypeScript for the Frontend
**Status:** Accepted

The frontend uses React with TypeScript. API clients, view models, and presentation state remain separate from backend persistence structures.

## ADR-004: Use .NET and Azure Functions for the Backend
**Status:** Accepted

The backend uses .NET hosted on Azure Functions. Functions provide HTTP and scheduled-trigger boundaries while the application and domain layers remain platform-independent.

## ADR-005: Use USGS as the Initial Data Source
**Status:** Accepted

USGS is the first external provider. Provider access is isolated behind a source adapter.

## ADR-006: Use Oracle for Persistence
**Status:** Accepted

Oracle is the initial database. Repository interfaces isolate persistence concerns and support indexed time, geographic, identity, and analytics queries.

## ADR-007: Identify Events by `source + external_id`
**Status:** Accepted

The combination of provider name and provider event identifier is the natural external identity. Ingestion uses an idempotent UPSERT on this key, and an existing event may be updated because provider records can be revised.

## ADR-008: Keep Earthquakes Independent from Regions
**Status:** Accepted

Regions are geographic query concepts. The earthquake entity is not permanently assigned to a region. Initial options are Costa Rica, Central America, Caribbean, and Global.

## ADR-009: Defer Queues and Microservices in V1
**Status:** Accepted

V1 uses direct application flows and scheduled or administrative HTTP-triggered ingestion. Messaging remains a future option for higher throughput, retry isolation, fan-out processing, or alert delivery.

## ADR-010: Design for Incremental Evolution
**Status:** Accepted

Stable contracts are used for source adapters, repositories, regional queries, and application use cases. Future features such as heat maps, analytics, clustering, multiple sources, alerts, observability, caching, and messaging must use those boundaries.

## ADR-011: Protect the Public Query Surface with a Server-Side Proxy
**Status:** Accepted

The React application must not contain an API key. The browser calls same-origin Vercel serverless functions, which add the Azure Functions host key (`x-functions-key`) and application key (`x-api-key`) from server-side environment variables. Azure query functions use `AuthorizationLevel.Function` and reject requests without both authorization layers.

This is a BFF boundary: Vercel is the public browser surface while the data API remains protected. Secrets must never be placed in `VITE_*` variables, HTML, JavaScript bundles, or API responses.

## ADR-012: Keep Ingestion Out of the Public Application API
**Status:** Accepted

Scheduled ingestion remains a Timer Trigger. Manual ingestion is available only through `POST /api/management/ingestion` with `AuthorizationLevel.Admin`, requiring the Functions host master key. React and the public proxy never call this route.

This prevents unauthenticated users from forcing expensive USGS and Oracle work.

## ADR-013: Apply Query Abuse Controls at Multiple Layers
**Status:** Accepted

The query API applies strict validation, a maximum period of 366 days, a maximum page size of 500, indexed bounded queries, DTOs without raw payloads, fixed-window per-client rate limiting, `Retry-After` responses, and no-store behavior for unauthorized responses. The frontend uses a 350-row page for Global and keeps regional queries at 100 results. The Vercel proxy adds short-lived shared caching for identical reads.

The in-memory limiter is an instance-local V1 defense. Production should add Azure Front Door or API Management WAF/rate limiting before the Function App; Redis or a distributed limiter can be introduced if multi-instance fairness becomes necessary. Credentials, provider payloads, function keys, and application keys must never be sent to the browser or written to logs.

## ADR-014: Harden the Frontend Dependency Supply Chain with pnpm
**Status:** Accepted

The frontend uses pnpm exclusively, with the version pinned through `packageManager` and direct dependency ranges pinned to exact versions. A committed `pnpm-lock.yaml` is required before a deployment is accepted, and CI must use `pnpm install --frozen-lockfile`.

The frontend `.npmrc` uses the official registry over TLS, verifies store integrity, requires strict peer dependencies, rejects exotic subdependencies, waits 24 hours before accepting newly published releases, and disables lifecycle scripts. This prevents install-time code execution by default. Any dependency that needs a build script requires an explicit security review and a deliberate policy change. TLS verification must never be disabled to work around certificate errors.

## ADR-015: Use a Bounded Seven-Day Default for the Event Map
**Status:** Accepted

The frontend loads stored events automatically when the page opens, using the protected query API's default seven-day window and a maximum of 100 results. The map displays those events as Leaflet markers, scales marker size by magnitude, fits the viewport to the returned coordinates, and shows the event origin time in UTC in each popup.

The seven-day default provides a useful initial view without requesting the entire database or making an unbounded public query. Users can narrow the view by region or minimum magnitude, while explicit API time-range parameters remain available for bounded historical queries.
