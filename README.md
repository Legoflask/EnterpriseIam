# Enterprise Multi-Tenant IAM & Document Extraction Engine

A high-performance, containerised multi-service ecosystem architecture using modern web patterns. This workspace manages secure tenant-isolated user access while running native background document workers.

## System Architecture

The environment relies on three tightly decoupled application layers deployed within a unified container network:

1. **EnterpriseIam Gateways (.NET 10 Web API):** Manages metadata configurations, system pipelines, and secure user spaces. Utilises EF Core with a dynamic global query filter that attaches a target TenantId to incoming database threads automatically.
2. **DocumentWorker Node (Python 3.12 + FastAPI):** A lightning-fast extraction worker utilizing pypdf for document processing operations and pyodbc for direct multi-tenant transaction monitoring. Runs async handlers on **Port 8000**.
3. **Database Grid (SQL Server 2022 Linux):** An enterprise-tier relational database system handling data multi-tenancy dynamically through data isolation strategies.

## Security Infrastructure

- **Authentication:** Bearer JWT tokens evaluated using custom policy engines.
- **API Documentation:** Custom BearerSecuritySchemeTransformer seamlessly integrated into a standalone, ultra-fast **Scalar UI documentation portal**.
- **Data Isolation:** Hardened boundaries utilizing automated application query filters to guarantee tenants can never read or mutate overlapping data structures.

## Getting Started

### Prerequisites

Ensure you have the following engine runtimes running locally:
- Docker Desktop
- Docker Compose v2

### Quickstart Local Deployment

1. **Clone the repository:**
   git clone https://github.com
   cd YOUR_REPOSITORY_NAME

2. **Establish Environment Configurations:**
   Create a local .env file in the root workspace folder to feed application strings safely to Docker:
   MSSQL_SA_PASSWORD=YourStrongSecurePassword123!

3. **Orchestrate and Run:**
   Execute the isolated cleaner build command directly within your shell window:
   docker compose build --no-cache; docker compose up --build

### Exposed Operational Gateways

- Ingestion Gateway Dashboard (Scalar UI): http://localhost:5069/scalar/v1
- FastAPI Processing Worker Tier: http://localhost:8000
- Relational Database Node: localhost:1433