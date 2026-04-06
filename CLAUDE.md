# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build Commands

All build tasks are driven by `build.ps1` (PowerShell):

```powershell
./build.ps1                                        # Full build: clean, restore, build, test, publish
./build.ps1 -Target Dotnet.Build                   # Compile only
./build.ps1 -Target Dotnet.Test                    # Run all tests
./build.ps1 -Target DockerCompose.StartDetached    # Start infrastructure + app in background
./build.ps1 -Target DockerCompose.Stop             # Stop all services
./build.ps1 -Target Prune                          # Clean all build artifacts and Docker resources
```

To run a single test, use `dotnet test` directly:

```bash
dotnet test src/<TestProject>/<TestProject>.csproj --filter "FullyQualifiedName~<TestName>"
```

## Code Formatting

CSharpier is the formatter:

```bash
dotnet tool restore
dotnet csharpier .
```

`TreatWarningsAsErrors: true` is set globally — the build will fail on any compiler warning.

## Architecture Overview

**Domain:** Shipment fulfillment orchestration. A `ShipmentProcess` coordinates carrier manifestation, document generation, and collection booking — potentially across multiple legs (e.g. domestic vs. international).

**Pattern:** Hexagonal Architecture (Ports & Adapters) + Event Sourcing + CQRS. All state changes are recorded as immutable events; state is reconstructed by replaying them.

### Layer Structure

```plaintext
EventSourcedPM               → ASP.NET Core Web API host + DI composition root
EventSourcedPM.Domain        → Aggregates, domain events, state reconstruction
EventSourcedPM.Messaging     → Shared message contracts (events, commands, models)
EventSourcedPM.Application   → Process Manager, sub-processes, application logic
EventSourcedPM.Configuration → DI configurators, settings
EventSourcedPM.Ports.*       → Abstract interfaces: EventStore, MessageBus, CarrierIntegration
EventSourcedPM.Adapters.*    → Concrete implementations (swap without changing application code)
```

### Adapters

| Port | Adapters |
| --- | --- |
| `Ports.EventStore` | `Adapters.MartenDbEventStore` (PostgreSQL via Marten) or `Adapters.KurrentDb` (EventStoreDB) |
| `Ports.MessageBus` | `Adapters.MassTransitMessageBus` (RabbitMQ via MassTransit) |
| `Ports.CarrierIntegration` | `Adapter.CarrierIntegrationStub` (test stub) |

The active event store adapter is selected at startup via `EventStoreConfigurator.cs`.

### Process Manager Pattern

`ShipmentProcess` is the central aggregate. It does not react to events passively (Saga style) — it is an explicit orchestrator that:

1. Receives commands via the message bus
2. Decides next steps based on replayed event state
3. Delegates to `ManifestationAndDocumentsSubprocess` and `CollectionBookingSubprocess`
4. Records all decisions as events

### API Endpoints

- `POST /{shipmentId}` — Start the process for a shipment
- `GET /{shipmentId}` — Get the current outcome

### Shipment ID Conventions (for testing/dev)

The stub carrier classifies behavior based on the shipment ID:

- Starts with `'1'` → domestic (single-leg)
- Otherwise → international (multi-leg, customs docs)
- Ends with `'1'` → fails at manifestation
- Ends with `'2'` → fails at collection booking
- Ends with `'3'` → fails then retries and succeeds
- Otherwise → succeeds

## Infrastructure

Docker Compose manages all services. Platform-specific compose files exist for `amd64` and `arm64`:

- **PostgreSQL** — event store (Marten adapter)
- **EventStoreDB** — alternative event store (KurrentDB adapter)
- **RabbitMQ** — message broker (management UI at port 15672)
- **Application** — ASP.NET Core API (port 43210)

## Key Configuration Files

- `global.json` — pins .NET SDK to 10.0.100
- `src/Directory.Build.props` — global build properties (target framework, warnings-as-errors)
- `src/Directory.Packages.props` — centralized NuGet version management
- `.editorconfig` — LF line endings, 4-space indent for C#, max line length 150
- `src/EventSourcedPM/appsettings.json` — runtime configuration defaults
