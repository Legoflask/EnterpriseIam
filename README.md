# Enterprise IAM - Multi-Tenant Document Extraction Platform

A production-ready, containerized multi-service platform for secure tenant isolation, user authentication, and automated document processing.

## System Architecture

The platform consists of three independent services running in Docker containers:

| Service | Technology | Port | Purpose |
|---------|-----------|------|---------|
| **API Gateway** | .NET 10 Core | 5069 | Tenant management, user authentication, document uploads |
| **Document Worker** | Python 3.12 + FastAPI | 8000 | PDF text extraction and processing |
| **Database** | SQL Server 2022 | 1433 | Multi-tenant data storage with tenant isolation |

### Key Features

- **Multi-Tenant Architecture**: Each tenant's data is automatically isolated at the database layer
- **JWT Authentication**: Secure token-based API access with tenant context
- **Document Processing**: Async PDF extraction with dedicated worker service
- **API Documentation**: Interactive Swagger UI at `/scalar/v1`
- **Data Persistence**: SQL Server with containerized volumes

## Prerequisites

- Docker Desktop (running)
- Git
- PowerShell 5.1+ (Windows) or Bash (Linux/Mac)

## Quick Start

### 1. Clone the Repository
```bash
git clone https://github.com/Legoflask/EnterpriseIam.git
cd EnterpriseIam
```

### 2. Configure Environment
```bash
cp .env.example .env
```

Edit `.env` and set `MSSQL_SA_PASSWORD` to a strong password. Never commit `.env` to Git.

### 3. Start the Stack
```bash
docker compose up -d --pull always
```

This starts all three services automatically:
- SQL Server database
- .NET API Gateway
- Python document worker

### 3. Verify Everything is Running
```bash
docker compose ps
```

All containers should show `Up` status.

## Using the API

### Access the API Documentation
Visit the interactive Swagger UI:
```
http://localhost:5069/scalar/v1
```

### Register a New Tenant
```powershell
curl -X POST "http://localhost:5069/api/auth/register-tenant" \
  -H "Content-Type: application/json" \
  -d '{
    "TenantName": "Acme Corp",
    "AdminEmail": "admin@acme.local",
    "Password": "YourSecurePassword@123"
  }'
```

### Login and Get JWT Token
```powershell
curl -X POST "http://localhost:5069/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "Email": "admin@acme.local",
    "Password": "YourSecurePassword@123"
  }'
```

### Upload a PDF Document
```powershell
curl -X POST "http://localhost:5069/api/documents/upload" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -F "file=@document.pdf"
```

## Available Endpoints

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/api/auth/register-tenant` | POST | Register a new tenant and admin user |
| `/api/auth/login` | POST | Authenticate and receive JWT token |
| `/api/documents/upload` | POST | Upload and extract PDF documents |
| `/openapi/v1.json` | GET | OpenAPI specification |
| `/scalar/v1` | GET | Interactive API documentation |

## Common Commands

### View Service Logs
```bash
# View all logs
docker compose logs -f

# View specific service
docker compose logs -f api-gateway
docker compose logs -f document-worker
docker compose logs -f database
```

### Stop the Stack
```bash
docker compose down
```

### Restart a Service
```bash
docker compose restart api-gateway
```

### View Database Records
```bash
docker exec enterprise_iam_db /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P YourStrong@Password123 \
  -d EnterpriseIamDb -C \
  -Q "SELECT * FROM Tenants"
```

## Project Structure

```
EnterpriseIam/
├── WebAPI/                  # .NET 10 API Gateway
│   ├── Controllers/         # API endpoints
│   ├── Models/              # Request/response DTOs
│   └── Program.cs           # Startup configuration
├── DocumentWorker/          # Python FastAPI worker
│   ├── app/                 # Application code
│   ├── Dockerfile           # Container image
│   └── requirements.txt      # Python dependencies
├── Core/                    # Shared entities
├── Infrastructure/          # Data access & services
├── docker-compose.yml       # Multi-container setup
└── README.md                # This file
```

## Database Schema

### Tenants
Stores each organization/customer
```sql
Id (GUID), Name (string), CreatedAtUtc (datetime)
```

### Users
User accounts with tenant isolation
```sql
Id (GUID), TenantId (GUID), Email (string), PasswordHash (string), IsActive (bool)
```

### Documents
Uploaded PDF files tracked by tenant
```sql
Id (GUID), TenantId (GUID), FileName (string), Status (string), UploadedAtUtc (datetime)
```

### ExtractedData
Extraction results linked to documents
```sql
Id (GUID), DocumentId (GUID), RawText (nvarchar(MAX)), ProcessedAtUtc (datetime)
```

## Troubleshooting

### Containers Won't Start
```bash
docker compose down
docker compose up -d --pull always
```

### Database Connection Error
```bash
docker logs enterprise_iam_db
```

### API Gateway Not Responding
```bash
docker logs net10_api_gateway
```

### Document Worker Extraction Fails
```bash
docker logs fastapi_document_worker
```

### Port Already in Use
Change ports in `docker-compose.yml`:
```yaml
ports:
  - "5069:5069"  # Change first number to unused port
```

## Security

### Environment Variables
- Never commit `.env` files to Git (already in `.gitignore`)
- Use `.env.example` as a template for developers
- Change `MSSQL_SA_PASSWORD` in production
- Use strong passwords (minimum 12 characters, mixed case, numbers, symbols)

### Production Deployment
- Change default credentials before deploying
- Use Docker secrets or external secret management (AWS Secrets Manager, Azure Key Vault)
- Enable HTTPS/TLS
- Restrict database port access
- Use network policies to isolate containers

### Development
- Default password: `YourStrong@Password123` (for local use only)
- Never expose API keys or tokens in code
- Use JWT tokens with short expiration times


```bash
docker compose build --no-cache
```

### Running with Live Logs
```bash
docker compose up
```
(Press `Ctrl+C` to stop)

### Accessing Container Shell
```bash
docker exec -it net10_api_gateway /bin/bash
docker exec -it fastapi_document_worker /bin/bash
docker exec -it enterprise_iam_db /bin/bash
```

## Environment Variables

Edit `docker-compose.yml` to customize:

```yaml
environment:
  - ASPNETCORE_ENVIRONMENT=Development
  - MSSQL_SA_PASSWORD=YourStrong@Password123
```

## License

[Add your license here]

## Support

For issues, feature requests, or questions, please open an issue on GitHub.
