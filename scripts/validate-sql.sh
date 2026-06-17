#!/usr/bin/env bash
set -euo pipefail

for file in database/migrations/*.sql; do
  echo "Validando $file"
  test -s "$file"
  sed -n '1,260p' "$file" >/dev/null

  if command -v psql >/dev/null 2>&1 && [[ -n "${DATABASE_URL:-}" ]]; then
    psql "$DATABASE_URL" --set ON_ERROR_STOP=on --single-transaction --file "$file"
  else
    echo "Aviso: psql ou DATABASE_URL indisponível; validação limitada a existência/leitura do arquivo."
  fi
done
