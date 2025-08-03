# Ignite Life

![CI - Run Tests](https://github.com/callan321/ignite-life-server/actions/workflows/ci.yml/badge.svg)

## 🛠️ Prerequisites (For Development)

### Windows and MacOS

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

Includes both **Docker Engine** and **Docker Compose**, so no extra installs needed. Make sure Docker Desktop is **running** before running commands.

### Linux Users

- Install [Docker Engine](https://docs.docker.com/engine/install/)  
- Install [Docker Compose](https://docs.docker.com/compose/install/) (if not bundled)

## 🚀 Development Setup  

To get started, follow these steps:

1. **Clone the repository**

    ```bash
    git clone https://github.com/callan321/ignite-life-server.git
    ```

2. **Navigate into the project folder**

    ```bash
    cd ignite-life-server
    ```

3. **Build and run the test environment**

    ```bash
    docker compose --env-file .env.test up --build --exit-code-from tests tests
    ```

    ✅ This command:
    - Builds all containers  
    - Starts a **PostgreSQL test database**  
    - Builds and runs all tests in Docker  
    - Leaves the test database running for further testing  

### ⚙️ After Initial Setup

Once the test database is running, you can run tests directly without rebuilding Docker:

```bash
dotnet test
```

> ⚠️ If you add or remove NuGet packages or change any .csproj project files, you must repeat Step 3 to rebuild the Docker image so dependencies are updated.
