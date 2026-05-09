# Run from backend-platform/ (this directory).
DOTNET ?= dotnet
API_PROJ := src/Platform.Api/Platform.Api.csproj
INFRA_PROJ := src/Platform.Infrastructure/Platform.Infrastructure.csproj
TESTS_PROJ := tests/Platform.IntegrationTests/Platform.IntegrationTests.csproj
UNIT_TESTS_PROJ := tests/Platform.UnitTests/Platform.UnitTests.csproj

.PHONY: help restore build test test-unit test-integration run \
	db-drop db-update db-recreate postgres-up postgres-down

.DEFAULT_GOAL := help

help:
	@echo "backend-platform — common commands"
	@echo ""
	@echo "  make restore          dotnet restore"
	@echo "  make build            dotnet build"
	@echo "  make test             all tests (unit + integration)"
	@echo "  make test-unit        unit tests only"
	@echo "  make test-integration integration tests only"
	@echo "  make run              run Platform.Api (Development)"
	@echo ""
	@echo "Database (EF Core — uses ConnectionStrings:Default from API config / env)"
	@echo "  make db-drop          drop database (--force, no prompt)"
	@echo "  make db-update        apply migrations"
	@echo "  make db-recreate      db-drop then db-update"
	@echo ""
	@echo "Postgres (see README.md)"
	@echo "  make postgres-up      docker compose up -d postgres"
	@echo "  make postgres-down    docker compose down"

restore:
	$(DOTNET) restore

build: restore
	$(DOTNET) build --no-restore

test: build
	$(DOTNET) test --no-build

test-unit: build
	$(DOTNET) test --no-build $(UNIT_TESTS_PROJ)

test-integration: build
	$(DOTNET) test --no-build $(TESTS_PROJ)

run:
	$(DOTNET) run --project $(API_PROJ)

db-drop:
	$(DOTNET) ef database drop --force \
		--project $(INFRA_PROJ) \
		--startup-project $(API_PROJ)

db-update:
	$(DOTNET) ef database update \
		--project $(INFRA_PROJ) \
		--startup-project $(API_PROJ)

db-recreate: db-drop db-update

postgres-up:
	docker compose up -d postgres

postgres-down:
	docker compose down
