# Architecture Decisions

This document records the main decisions for Earthquake Monitor and the reasoning behind them.

## ADR-001: Use a Modular Monolith for V1

**Status:** Accepted

The first release will use one backend deployment organized into explicit modules. This keeps deployment, debugging, local development, and operational ownership simple while preserving boundaries that can later support extraction into services if justified.

Microservices are deferred because the initial domain and expected workload do not yet require independent scaling or independent deployment.

## ADR-002: Apply Clean Architecture

**Status:** Accepted

The domain and use cases must not depend on Azure Functions, Oracle, USGS, or framework-specific details. Dependencies point inward toward business rules. Infrastructure implements interfaces defined by the application or domain boundaries.

This enables focused unit tests, provider replacement, and clearer evolution of the system.

## ADR-003: Use React and TypeScript for the Frontend

**Status:** Accepted

The frontend will use React with TypeScript. Components, API clients, view models, and presentation state should remain separate enough to support regional dashboards, event detail pages, heat maps, and analytics views without coupling the UI to backend persistence structures.

## ADR-004: Use .NET and Azure Functions for the Backend

**Status:** Accepted

The backend will use .NET hosted on Azure Functions. Functions provide the initial HTTP and scheduled-trigger boundaries while allowing the application and domain layers to remain platform-independent.

## ADR-005: Use USGS as the Initial Data Source

**Status:** Accepted

USGS is the first external provider. Provider access must be isolated behind a source adapter so that additional seismic sources can be added without changing core business logic.

## ADR-006: Use Oracle for Persistence

**Status:** Accepted

Oracle is the initial database. Repository interfaces isolate persistence concerns, and schema decisions should support indexed time-based queries, geographic filtering, source identity, and future analytics.

## ADR-007: Identify Events by `source + external_id`

**Status:** Accepted

The combination of the provider name and provider event identifier is the natural external identity of an event. Ingestion must use an idempotent UPSERT on this key. Reprocessing the same provider response must not create duplicates.

An existing event may be updated because provider records can be revised after initial publication.

## ADR-008: Keep Earthquakes Independent from Regions

**Status:** Accepted

Regions are geographic query concepts. The earthquake entity must not be permanently assigned to one region. Initial region options are Costa Rica, Central America, Caribbean, and Global. Geographic membership may evolve from simple configured rules to richer boundary or geospatial capabilities.

## ADR-009: Defer Queues and Microservices in V1

**Status:** Accepted

V1 will use direct application flows and scheduled or HTTP-triggered ingestion. Messaging, queues, and service decomposition remain future options for higher throughput, retry isolation, fan-out processing, or alert delivery.

## ADR-010: Design for Incremental Evolution

**Status:** Accepted

The system should expose stable contracts for source adapters, repositories, regional queries, and application use cases. Future features—heat maps, analytics, clustering, multiple sources, alerts, observability, caching, and messaging—should be added through those boundaries rather than by leaking infrastructure details into the domain.

