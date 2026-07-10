# SolidarityGrid

🇺🇸 English Documentation (default)

🇨🇴 [Leer documentación en español](README.es.md)

[![.NET 8](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Docker](https://img.shields.io/badge/Docker-Compose-blue.svg)](https://www.docker.com/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-red.svg)](https://www.microsoft.com/sql-server)
[![Architecture](https://img.shields.io/badge/Architecture-Clean%20Architecture-green.svg)]()

---

A distributed payment processing platform built without a central coordinator or external messaging system.

Developed in .NET 8 as a proof of concept for the **Payment Mesh Resilience** challenge, where multiple nodes collaborate to process transactions resiliently, detect peer failures, and automatically recover unfinished work, ensuring each payment is processed exactly once.

---

# Table of Contents

- [Objectives](#objectives)
- [Key Features](#key-features)
- [Architecture](#architecture)
- [Processing Flow](#processing-flow)
- [Failure Recovery](#failure-recovery)
- [Project Structure](#project-structure)
- [Technologies](#technologies)
- [Configuration](#configuration)
- [Getting Started](#getting-started)
- [API](#api)
- [Failover Simulation](#failover-simulation)
- [Observability](#observability)
- [Author](#author)
- [Local Pipeline](#local-pipeline)

---

# Objectives

The solution was designed to demonstrate the following technical capabilities:

- Distributed transaction processing.
- Coordination between multiple nodes without a permanent leader.
- Automatic recovery of transactions when a node becomes unavailable.
- Prevention of duplicate processing through optimistic concurrency.
- Direct node-to-node communication over HTTP.
- Fully automated deployment with a single Docker Compose command.
- A maintainable, loosely coupled architecture designed for future evolution.
---

# Key Features

- Built following the principles of Clean Architecture.
- Asynchronous payment processing using Background Services.
- Peer-to-peer communication through HTTP Heartbeats.
- Automatic detection of inactive nodes.
- Automatic recovery of orphaned transactions (Failover).
- Optimistic Concurrency using `RowVersion`.
- Exclusive transaction ownership through Lease Ownership.
- Centralized persistence with SQL Server.
- Automatic API documentation with Swagger.
- Reproducible deployment using Docker Compose.

---

# Architecture

The platform consists of three identical nodes running the exact same application.

There is no master node or central coordinator. Each instance can accept incoming requests, claim pending transactions, process payments, and take over unfinished work from another node when a failure is detected.

All nodes share the same database and periodically exchange Heartbeats to monitor the health of the cluster.

```mermaid
flowchart TD

Client([Client])

Client --> LB

LB["Round Robin / Random"]

LB --> NodeA
LB --> NodeB
LB --> NodeC

subgraph SolidarityGrid Cluster

NodeA["Node A"]
NodeB["Node B"]
NodeC["Node C"]

NodeA <--> NodeB
NodeB <--> NodeC
NodeC <--> NodeA

end

NodeA --> SQL[(SQL Server)]
NodeB --> SQL
NodeC --> SQL
```
## Clean Architecture

The solution is organized following the principles of Clean Architecture, with a clear separation of responsibilities across the application's layers.

This approach keeps the business rules independent of infrastructure concerns, improving maintainability, enabling future evolution, and facilitating unit testing.

```
Presentation (API)
        │
        ▼
Application
        │
        ▼
Domain
        ▲
        │
Infrastructure
```

Each project has a specific responsibility:

| Project | Responsibility |
|----------|----------------|
| SolidarityGrid.Api | Exposes the REST API endpoints, application configuration, dependency injection, and Swagger documentation. |
| SolidarityGrid.Application | Contains application use cases, application services, and contracts used throughout the solution. |
| SolidarityGrid.Domain | Defines the domain model, entities, enumerations, exceptions, and business rules. |
| SolidarityGrid.Infrastructure | Implements persistence, Entity Framework Core, repositories, Background Services, and inter-node communication. |


## Node Responsibilities

Each application instance is capable of:

- Exposing the REST API.
- Registering new transactions.
- Claiming pending payments.
- Processing transactions asynchronously.
- Publishing periodic Heartbeats.
- Detecting inactive nodes.
- Recovering abandoned transactions.
- Completing transaction processing safely.

This approach eliminates single points of failure and enables horizontal scalability by allowing additional instances to be added without modifying the business logic.

---

# Key Design Decisions

| Decision | Rationale |
|----------|-----------|
| Clean Architecture | Separates business rules from infrastructure, improving maintainability and testability. |
| Shared SQL Server | Centralizes application state and simplifies distributed coordination. |
| BackgroundService | Decouples HTTP request handling from payment processing, preventing request blocking. |
| HTTP Heartbeats | Detect inactive nodes without relying on external coordination tools. |
| Optimistic Concurrency (`RowVersion`) | Prevents duplicate processing using SQL Server's native concurrency control. |
| Docker Compose | Brings up the entire platform with a single command. |
| Inter-node HTTP Communication | Reduces architectural complexity and eliminates the need for external message brokers. |

---

## Asynchronous Processing

Payment processing does not occur during the HTTP request.

When a client submits a new transaction, the API only validates the request and persists it with a **Pending** status, immediately returning a response to the client.

The actual processing is performed later by a `BackgroundService`, allowing the API to remain lightweight and maintain low latency.

### Benefits

- Fast responses to clients.
- Reduced HTTP connection blocking time.
- Improved capacity to handle multiple concurrent requests.
- Clear separation between request acceptance and transaction processing.

---

## Inter-Node Communication

The nodes form a Peer-to-Peer (P2P) network.

There is no central coordinator responsible for distributing the workload.

Each instance is configured with the addresses of its peer nodes and periodically communicates with them through HTTP requests.

```text
Node A  ←────→  Node B
   ▲               ▲
   │               │
   └──────→────────┘
         Node C
```

This approach eliminates single points of failure and enables horizontal scalability by allowing new instances to be added without changing the coordination logic.

---

## Heartbeat

Each node periodically publishes a Heartbeat to indicate that it is active while processing transactions.

The Heartbeat serves as a lightweight health monitoring mechanism between nodes and forms the foundation for failure detection within the cluster.

If a node fails to update its Heartbeat within the configured timeout period, the remaining nodes consider it unavailable.

```text
Node A

Heartbeat
Heartbeat
Heartbeat
Heartbeat
...
X

Timeout

↓

Node B detects the missing Heartbeats

↓

Node B initiates the recovery process
```

### Objectives

- Detect failed nodes.
- Identify abandoned transactions.
- Automatically initiate the recovery process.
- Eliminate the need for manual intervention.

---

## Lease Ownership

To prevent multiple nodes from processing the same transaction simultaneously, each payment is assigned a temporary owner (Lease Owner).

When a node claims a pending transaction, it records its identity as the current processing owner.

As long as the lease remains valid, no other node is allowed to process that transaction.

```text
TX-100

Owner = node-b

Status = Processing
```

If the current owner becomes unavailable and its Heartbeat expires, another node can reclaim the transaction and assume ownership.

This approach prevents race conditions without relying on external distributed locking mechanisms.\

---

## Optimistic Concurrency

Data consistency is ensured through Optimistic Concurrency using a `RowVersion` column managed by SQL Server.

Each update verifies that the record has not been modified by another process since it was last read.

If two nodes attempt to update the same transaction simultaneously, only one operation will succeed. The other will receive a concurrency exception and discard its processing attempt.

### Benefits

- Prevents long-running database locks.
- Reduces contention between concurrent processes.
- Ensures data consistency even under high concurrency.
- Leverages SQL Server's built-in concurrency control mechanisms.

---

## Idempotency

One of the primary requirements of the challenge is to ensure that a payment is never processed more than once.

The solution achieves idempotency through the combination of:

- Transaction status.
- Lease Ownership.
- Optimistic Concurrency.
- Validation checks before processing begins.

Once a transaction is marked as **Completed**, it can no longer be claimed or processed again, even if another node attempts to do so.

---

# Failure Recovery

Automatic recovery is the core capability of the solution.

When a node becomes unavailable while processing a payment, the remaining nodes continue monitoring the cluster through periodic Heartbeats.

If the configured timeout expires, any available node can reclaim transactions left in the **Processing** state, reassign ownership, and complete the processing automatically without human intervention.

```mermaid
sequenceDiagram

participant A as Node A
participant B as Node B
participant DB as SQL Server

A->>DB: Claims TX-100
A->>DB: Status = Processing

A--xB: Heartbeat stopped

Note over B: Timeout detected

B->>DB: Claims TX-100

DB-->>B: Lease granted

B->>DB: Processes payment

B->>DB: Status = Completed
```

The solution does not implement formal consensus algorithms such as Raft or Paxos. Instead, it relies on a lightweight coordination strategy based on a shared database, periodic Heartbeats, Lease Ownership, and Optimistic Concurrency. This approach is sufficient for the scope of the challenge, keeping the architecture simple while avoiding external coordination services.
___

# Project Structure

```text
SolidarityGrid
│
├── .github
├── .vs
├── docker
├── docs
├── src
│   ├── SolidarityGrid.Api
│   │   ├── Contracts
│   │   │   ├── Request
│   │   │   └── Responses
│   │   ├── Controllers
│   │   ├── Properties
│   │   ├── appsettings.json
│   │   ├── DependencyInjection.cs
│   │   ├── Dockerfile
│   │   ├── GlobalUsingApi.cs
│   │   ├── Program.cs
│   │   └── SolidarityGrid.Api.http
│   │
│   ├── SolidarityGrid.Application
│   │   ├── Abstractions
│   │   ├── DTOs
│   │   ├── Mappings
│   │   ├── Services
│   │   ├── DependencyInjection.cs
│   │   └── GlobalUsingApplication.cs
│   │
│   └── SolidarityGrid.Infrastructure
│       ├── Configuration
│       ├── HostedServices
│       ├── Mesh
│       ├── Migrations
│       ├── Persistence
│       ├── Repositories
│       ├── DependencyInjection.cs
│       └── GlobalUsingsInfrastructure.cs
│
├── tests
├── .gitignore
├── docker-compose.yml
└── README.md
```
___

# Technologies

The solution was built using technologies from the .NET ecosystem, prioritizing simplicity, maintainability, and ease of deployment.

| Technology | Version | Purpose |
|------------|---------|---------|
| .NET | 8 | Primary development platform |
| ASP.NET Core | 8 | REST API |
| Entity Framework Core | 8 | Data persistence |
| SQL Server | 2022 | Shared database |
| Docker | 27+ | Containerization |
| Docker Compose | v2 | Local orchestration |
| Swagger / OpenAPI | 3.0 | API documentation |

---

# Configuration

Each application instance has its own configuration, defining its identity within the cluster and the parameters used to coordinate distributed processing.

## Node Configuration

```json
{
  "Node": {
    "NodeName": "node-a",
    "HeartbeatTimeoutSeconds": 10,
    "ProcessingIntervalSeconds": 5,
    "PeerNodes": [
      "http://node-b:8080",
      "http://node-c:8080"
    ]
  }
}
```

## Parameters

| Property | Description |
|----------|-------------|
| NodeName | Unique identifier of the node within the cluster. |
| HeartbeatTimeoutSeconds | Maximum time allowed without receiving Heartbeats before a node is considered unavailable. |
| ProcessingIntervalSeconds | Interval used by the Background Service to scan for new pending transactions. |
| PeerNodes | List of peer nodes used to monitor the health and availability of the cluster. |

---

# Getting Started

## Prerequisites

Before running the solution, ensure the following tools are installed:

- .NET 8 SDK
- Docker Desktop
- Docker Compose

## Clone the Repository

```bash
git clone https://github.com/DavidV-2/SolidarityGrid.git

cd SolidarityGrid
```

## Start the Platform

The entire platform can be started with a single command.

```bash
docker compose up --build
```

This command automatically creates:

- SQL Server database.
- Docker internal network.
- Node A.
- Node B.
- Node C.

No additional manual configuration is required.

---
## Containers

Once the environment is running, the following services should be available:

| Container | Purpose |
|-----------|---------|
| solidaritygrid-node-a | Processing node |
| solidaritygrid-node-b | Processing node |
| solidaritygrid-node-c | Processing node |
| solidaritygrid-sql | SQL Server database |

You can verify the running containers using:

```bash
docker ps
```

# Swagger Access

Each node exposes its own OpenAPI documentation.

| Node | URL |
|------|-----|
| Node A | http://localhost:8081/swagger |
| Node B | http://localhost:8082/swagger |
| Node C | http://localhost:8083/swagger |

Although any of the three nodes can receive requests, using a single node is sufficient for testing purposes.

---
# API

## Create a Payment

Registers a new payment processing request.

### Endpoint

```http
POST /api/payments
```

### Request

```json
{
  "transactionId": "TX001",
  "amount": 150.00,
  "currency": "USD"
}
```

### Response

```http
202 Accepted
```

The API responds immediately after registering the transaction.

Payment processing continues asynchronously through a `BackgroundService`.

---

## Get Payments

Retrieves the list of registered transactions along with their current status.

### Endpoint

```http
GET /api/payments
```

### Available Information

- Transaction identifier.
- Current status.
- Processing node.
- Creation timestamp.
- Processing start timestamp.
- Completion timestamp.

### Example

| Transaction | Status | Owner |
|-------------|--------|-------|
| TX001 | Completed | node-a |
| TX002 | Processing | node-c |
| TX003 | Pending | - |

---

## Cluster Status

Returns the current health status of the cluster by identifying which nodes are available.

### Endpoint

```http
GET /api/nodes/status
```

### Response

```json
{
  "aliveNodes": [
    "node-a",
    "node-b",
    "node-c"
  ],
  "deadNodes": [],
  "totalNodes": 3
}
```

This endpoint is useful for verifying the Heartbeat mechanism during resilience and failover testing.

---

## 5. Automatic Recovery

One of the available nodes will reclaim any transactions left in the **Processing** state and continue processing them until completion.

No manual intervention is required.

---

## Expected Result

After querying the endpoint again:

```http
GET /api/payments
```

The transactions originally assigned to the failed node should now appear with the following status:

```text
Completed
```

and be owned by a different processing node.

This behavior demonstrates the cluster's ability to automatically recover from unexpected node failures.

---

# Observability

One of the evaluation criteria for this challenge is the ability to clearly understand the behavior of the distributed system through its logs.

For this reason, each node records the most relevant events throughout the lifecycle of a transaction.

## Logged Events

- Transaction processing started.
- Transaction claimed.
- Heartbeat published.
- Inactive node detected.
- Transaction recovery initiated.
- Transaction processing completed.
- Concurrency conflicts.
- Transaction state changes.

## Example of Normal Execution

```text
[node-a] Payment TX-100 registered.

[node-a] Transaction TX-100 claimed.

[node-a] Processing transaction TX-100.

[node-a] Transaction TX-100 completed successfully.
```

## Recovery Example

```text
[node-a] Processing transaction TX-200...

[node-b] Heartbeat timeout detected for node-a.

[node-b] Reclaiming transaction TX-200.

[node-b] Transaction TX-200 completed successfully.
```

These logs make it possible to reconstruct the complete lifecycle of a transaction and simplify the analysis of the system's behavior under high concurrency and failure scenarios.

---

# Author

**David Estiven Vélez González**

**Full-Stack .NET Developer**

**Core Technologies**

- C#
- .NET 8
- ASP.NET Core
- Entity Framework Core
- SQL Server
- Docker
- Clean Architecture
- REST APIs
- Distributed Systems

___

# Local Pipeline

```powershell
Write-Host "1. Starting the SolidarityGrid Mesh network..." -ForegroundColor Cyan
docker compose up -d --build

Write-Host "2. Waiting for containers and SQL Server to initialize..." -ForegroundColor Yellow
Start-Sleep -Seconds 15

Write-Host "3. Simulating concurrent payment injection (Stress Test)..." -ForegroundColor Cyan
1..5 | ForEach-Object {
     $body = @{
         transactionId = "TX$($_)"
         amount = $_ * 15
         currency = "ARC"
     } | ConvertTo-Json

     Invoke-RestMethod `
         -Method POST `
         -Uri "http://localhost:8081/api/Payments" `
         -ContentType "application/json" `
         -Body $body
}

Write-Host "4. [CHAOS] Abruptly stopping Node A..." -ForegroundColor Red
docker stop solidaritygrid-node-a

Write-Host "5. Monitoring the failover cluster logs (Node B and Node C)..." -ForegroundColor Green
docker compose logs -f node-b node-c
```