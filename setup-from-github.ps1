# Enterprise IAM - GitHub Clone and Setup Script
# Clones repo, sets up Docker, and runs the stack

param(
    [string]$GitHubUrl = "https://github.com/YOUR_USERNAME/EnterpriseIam.git",
    [string]$Branch = "main"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Enterprise IAM - GitHub Setup" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# ============================================
# 1. CLONE REPOSITORY
# ============================================
Write-Host "[1] Cloning repository from GitHub..." -ForegroundColor Yellow

if (-not (Test-Path "EnterpriseIam")) {
    git clone -b $Branch $GitHubUrl EnterpriseIam
    if ($LASTEXITCODE -ne 0) {
        Write-Host "FAIL - Git clone failed" -ForegroundColor Red
        exit 1
    }
    Write-Host "OK - Repository cloned" -ForegroundColor Green
}
else {
    Write-Host "OK - Repository already exists, pulling latest..." -ForegroundColor Green
    Push-Location EnterpriseIam
    git pull origin $Branch
    Pop-Location
}

Write-Host ""

# ============================================
# 2. NAVIGATE TO PROJECT
# ============================================
Write-Host "[2] Entering project directory..." -ForegroundColor Yellow
Push-Location EnterpriseIam

Write-Host "Current directory: $(Get-Location)" -ForegroundColor DarkGray
Write-Host "OK - In project root" -ForegroundColor Green

Write-Host ""

# ============================================
# 3. START DOCKER COMPOSE
# ============================================
Write-Host "[3] Starting Docker Compose stack..." -ForegroundColor Yellow

docker compose down 2>&1 | Out-Null
Start-Sleep -Seconds 2

docker compose up -d --pull always
if ($LASTEXITCODE -ne 0) {
    Write-Host "FAIL - Docker compose failed" -ForegroundColor Red
    exit 1
}

Write-Host "OK - Docker stack started" -ForegroundColor Green
Start-Sleep -Seconds 5

Write-Host ""

# ============================================
# 4. VERIFY CONTAINERS
# ============================================
Write-Host "[4] Verifying containers..." -ForegroundColor Yellow

$containers = docker ps --filter "status=running" --format "table {{.Names}}"

if ($containers -match "enterprise_iam_db") {
    Write-Host "OK - SQL Server running" -ForegroundColor Green
}
else {
    Write-Host "FAIL - SQL Server not running" -ForegroundColor Red
}

if ($containers -match "net10_api_gateway") {
    Write-Host "OK - API Gateway running" -ForegroundColor Green
}
else {
    Write-Host "FAIL - API Gateway not running" -ForegroundColor Red
}

if ($containers -match "fastapi_document_worker") {
    Write-Host "OK - Document Worker running" -ForegroundColor Green
}
else {
    Write-Host "FAIL - Document Worker not running" -ForegroundColor Red
}

Write-Host ""

# ============================================
# 5. DISPLAY ENDPOINTS
# ============================================
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Setup Complete!" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Available Endpoints:" -ForegroundColor Green
Write-Host "  API Gateway:        http://localhost:5069" -ForegroundColor DarkGray
Write-Host "  Swagger UI:         http://localhost:5069/scalar/v1" -ForegroundColor DarkGray
Write-Host "  Document Worker:    http://localhost:8000" -ForegroundColor DarkGray
Write-Host "  SQL Server:         localhost:1433" -ForegroundColor DarkGray
Write-Host ""

Write-Host "Useful Commands:" -ForegroundColor Green
Write-Host "  View logs:          docker compose logs -f [service]" -ForegroundColor DarkGray
Write-Host "  Stop stack:         docker compose down" -ForegroundColor DarkGray
Write-Host ""

Pop-Location
