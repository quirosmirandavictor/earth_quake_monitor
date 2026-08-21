Earthquake Monitor

Architecture:
- Modular Monolith
- Clean Architecture
- .NET + Azure Functions backend
- React + TypeScript frontend
- Oracle persistence
- USGS as initial earthquake source

Regions:
- Costa Rica
- Central America
- Caribbean
- Global

Principles:
- Earthquake is region-independent
- Region is a geographic query concept
- Idempotent ingestion using source + external_id
- USGS events can be updated after initial ingestion
- Start simple; no queues/microservices in V1 unless justified

Repository conventions:
- All commit messages must be written in English
- Update README.md whenever an important deployment change is made or new functionality is added
