FROM postgres:17

COPY migrations /migrations
COPY scripts/run-migrations.sh /docker-entrypoint-initdb.d/run-migrations.sh

RUN chmod +x /docker-entrypoint-initdb.d/run-migrations.sh

EXPOSE 5432