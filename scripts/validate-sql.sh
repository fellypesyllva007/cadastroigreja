#!/usr/bin/env bash
set -euo pipefail

for file in database/migrations/*.sql; do
  echo "Validando $file"
  sed -n '1,220p' "$file" >/dev/null
  test -s "$file"
done
