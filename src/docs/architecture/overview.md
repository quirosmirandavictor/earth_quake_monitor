# Earthquake Monitor Architecture Overview

## Purpose

Earthquake Monitor collects earthquake events from external seismic data providers, stores normalized event data, and presents region-focused information through a web application. The initial release prioritizes a small, understandable, and reliable foundation that can evolve without prematurely introducing distributed-system complexity.

## Architectural Style

The system is a **Modular Monolith** organized according to **Clean Architecture** principles.

- The monolith is deployed as a .NET Azure Functions backend and a React frontend.
- Backend modules are separated by business capability and communicate through explicit application contracts.
- Domain logic remains independent of Azure Functions, Oracle, USGS, HTTP clients, and other infrastructure concerns.
- The first version uses direct, synchronous application flows. Queues and microservices are future options, not V1 requirements.

## High-Level Components

### Frontend

- React with TypeScript.
- Presents the event list and an interactive Leaflet map with magnitude-scaled markers and event popups.
- Loads stored events from the default seven-day query window when the page opens.
- Displays the event origin date and time in UTC in both the list and map popup.
- Supports Global, Costa Rica, Central America, and Caribbean region filters, plus a minimum magnitude filter.
- Calls backend APIs rather than accessing Oracle or USGS directly.
- Keeps presentation state and API concerns separate from domain rules.

### Backend

- .NET running on Azure Functions.
- Exposes HTTP-triggered functions or an equivalent API boundary.
- Contains domain, application, infrastructure, and API concerns in separate layers.
- Coordinates ingestion, querying, normalization, filtering, and future alert workflows.

### Persistence

- Oracle is the initial persistence provider.
- Earthquake records are stored in a normalized internal model.
- Ingestion is idempotent through an UPSERT keyed by `source + external_id`.
- Provider-specific payloads may be retained for traceability and future reprocessing.

### Initial External Source

- USGS is the initial earthquake data source.
- A source adapter translates USGS responses into the internal earthquake model.
- The rest of the system depends on the adapter contract, not on USGS-specific formats.

## Core Domain Concepts

### Earthquake

An earthquake is a global, source-independent domain entity. It should not contain a region assignment as an intrinsic property. Typical data includes:

- Internal identifier.
- Source and external source identifier.
- Origin time.
- Latitude, longitude, and depth.
- Magnitude and magnitude type when available.
- Place or human-readable description.
- Event URL and source metadata.
- Created and updated timestamps.

### Region

A region is a geographic query and presentation concept, not an ownership attribute of an earthquake. Initial supported regions are:

- Costa Rica.
- Central America.
- Caribbean.
- Global.

Region membership should be calculated from geographic rules, configured boundaries, or a future geographic service. This keeps the earthquake model reusable and allows one event to appear in multiple regional views when appropriate.

## Main Flows

### Ingestion

1. An ingestion trigger starts a collection run.
2. The USGS adapter requests events for the configured time window.
3. The adapter validates and maps provider data to the internal model.
4. The application layer applies deduplication and update rules.
5. Oracle performs an idempotent UPSERT using `source + external_id`.
6. The run records operational outcome and error information.

USGS events may change after first publication. Therefore, a matching event must be updated when relevant provider fields change rather than treated as an immutable duplicate.

### Querying

1. A client requests events by time range, magnitude, location, or region.
2. The API validates query parameters.
3. The application layer resolves geographic filtering and use-case rules.
4. The repository queries Oracle.
5. The API returns a stable DTO designed for frontend consumption.

When no explicit time range is supplied, the query API uses the last seven days and returns up to 100 events ordered by origin time descending. This default keeps the initial map focused and bounded while preserving explicit time-range queries for historical exploration within the API limits.

## V1 Boundaries

V1 should include the core ingestion and query path, basic regional filtering, persistence, configuration, validation, logging, and automated tests. It should not require queues, event buses, microservices, or a complex workflow engine unless a concrete operational requirement emerges.

## Evolution Direction

The architecture leaves clear extension points for heat maps, analytics, clustering, additional data sources, alerts, observability, caching, and asynchronous messaging. These capabilities can be introduced as new modules or infrastructure adapters while preserving the domain model and existing application contracts.
