# Ignite Life

## 🛠️ Prerequisites (For Development)

### Windows and MacOS

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

Includes both **Docker Engine** and **Docker Compose**, so no extra installs needed. Make sure Docker Desktop is **running** before running commands.

### Linux Users

- Install [Docker Engine](https://docs.docker.com/engine/install/)  
- Install [Docker Compose](https://docs.docker.com/compose/install/) (if not bundled)

## Initial setup  

Clone the repository and build Docker images:

```bash
git clone TODO
cd TODO
docker compose build
```

## 🖥️ Running the Server

Start the backend API and database:

```bash
docker compose up ignite-api  
```

API runs at:  

- <http://localhost:8080>  
- <https://localhost:8081>  

Stop the server:

```bash
docker compose down  
```

## 🧪 Running Tests

### Option 1 - Clean, isolated run

```bash
docker compose up --abort-on-container-exit tests  
```

### Option 2 - Fast iterative testing

start test database

```bash
docker compose up -d db_test
```

run tests

```bash
docker compose run --rm tests  
```

stop containers

```bash
docker compose down
```

## 🔄 Rebuilding Images

Rebuild when you add/remove NuGet packages or modify Dockerfiles:

```bash
docker compose build
```
