FROM postgres:17

COPY migrations /migrations
COPY scripts/run-migrations.sh /docker-entrypoint-initdb.d/run-migrations.sh

RUN chmod +x /docker-entrypoint-initdb.d/run-migrations.sh

ENV POSTGRES_USER=guilhermeuser
ENV POSTGRES_PASSWORD=LIbolo0$
ENV POSTGRES_DB=database

EXPOSE 5432