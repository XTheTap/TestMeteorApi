version: "3.9"
services:
  api:
    build:
      context: ./src/WebApi
    container_name: webapi
    ports:
      - "9999:8078"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development

  worker:
    build:
      context: ./src/WorkerService
    container_name: workerservice
    depends_on:
      - api

  frontend:
    build:
      context: ./src/Frontend
    container_name: frontend
    ports:
      - "5173:80"
    depends_on:
      - api
