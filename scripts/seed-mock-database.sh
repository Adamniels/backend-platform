#!/usr/bin/env bash
# Truncates all public application data (preserves __EFMigrationsHistory), then seeds mock data.
# Requires: PostgreSQL reachable; schema must already be migrated.
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"
exec dotnet run --project "src/Platform.DevData/Platform.DevData.csproj" -- "$@"
