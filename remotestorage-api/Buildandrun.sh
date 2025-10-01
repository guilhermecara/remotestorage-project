docker build -t remotestorage-api:latest .
docker run -d -p 5050:5050 --name remotestorage-api-container-03 remotestorage-api:latest 