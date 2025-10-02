#!/bin/bash
set -e

for f in /migrations/*.sql; do
  echo "Running migration: $f"
  psql -U "$POSTGRES_USER" -d "$POSTGRES_DB" -f "$f"
done
echo "Migrations completed."
