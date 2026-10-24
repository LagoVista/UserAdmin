#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
project="$repo_root/tests/LagoVista.UserAdmin.Mongo.IntegrationTests/LagoVista.UserAdmin.Mongo.IntegrationTests.csproj"

echo "Running opt-in UserAdmin Mongo integration tests."
echo "The test assembly owns one bounded Mongo 8 container for this invocation and removes it on completion."
dotnet test "$project" --filter "TestCategory=MongoIntegration" "$@"
