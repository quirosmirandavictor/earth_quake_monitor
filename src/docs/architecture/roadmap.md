# Architecture Roadmap

## Phase 1: V1 Foundation

- Create the .NET solution with Clean Architecture layers.
- Define the earthquake domain model and source identity rules.
- Add the USGS source adapter.
- Implement scheduled ingestion and operator-only manual ingestion.
- Implement Oracle persistence with an idempotent UPSERT on `source + external_id`.
- Support updates to previously ingested USGS events.
- Expose validated HTTP query endpoints protected by Function authorization and a server-side application key.
- Add React and TypeScript frontend foundations with a Vercel server-side proxy.
- Provide event lists, event details, time-range filters, magnitude filters, analytics, the initial region filters, and a bounded event map: Costa Rica, Central America, Caribbean, and Global.
- Add configuration management, structured logging, error handling, rate limiting, query abuse controls, and automated tests.

### Phase 1 operational hardening still required

- Deploy the frontend proxy with server-side secrets only.
- Put Azure Front Door or API Management WAF/rate limiting in front of the Function App.
- Add distributed rate limiting if the Function App scales to multiple instances.
- Add CI/CD, production secret management, and automated database migrations.

## Phase 2: Product and Geographic Capabilities

- Improve regional boundary definitions and geographic filtering.
- Introduce heat maps using event density and magnitude.
- Add pagination, sorting, and optimized query projections.
- Add caching for frequently requested regional and time-range queries.

## Phase 3: Analytics and Enrichment

- Add historical analytics and trend summaries.
- Add configurable earthquake clustering for sequences and spatial-temporal patterns.
- Add provider metadata and enrichment workflows.
- Introduce data-quality checks and reprocessing tools.

## Phase 4: Multiple Sources

- Add additional source adapters behind the existing source contract.
- Normalize differences in magnitude scales, locations, timestamps, and event status.
- Improve source reconciliation where multiple providers represent the same physical event.
- Extend provenance and audit data without changing the core earthquake concept.

## Phase 5: Alerts and Messaging

- Define alert rules by region, magnitude, depth, and event status.
- Add notification channels and user preferences.
- Introduce queues or messaging when asynchronous processing, retries, fan-out, or alert delivery justify them.
- Keep messaging concerns outside the core domain model.

## Phase 6: Operational Maturity

- Expand observability with metrics, traces, dashboards, and actionable alerts.
- Add ingestion-run monitoring and source health checks.
- Improve resilience with bounded retries, rate-limit handling, and circuit-breaking where needed.
- Reassess module scaling and service extraction only when measured workload, team ownership, or reliability requirements justify it.

## Guiding Rule

Each phase should be driven by a demonstrated product or operational need. The system should gain capability incrementally while keeping the domain model, source contracts, and regional query concepts stable.
