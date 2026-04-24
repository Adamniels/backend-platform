# backend-platform

## Postgres via Docker

Start Postgres:

```bash
docker compose up -d postgres
```

Stop Postgres:

```bash
docker compose down
```

Default connection string used by the API:

`Host=localhost;Port=5432;Database=platform;Username=platform;Password=platform`

## Run API

```bash
cd src/Platform.Api
dotnet run
```

The app applies EF Core migrations on startup.

In Development, the API will also load `backend-platform/.env` (or `src/Platform.Api/.env`) automatically if present.
Shell environment variables still take precedence over `.env` values.

