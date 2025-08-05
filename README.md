# Ignite Life

![CI - Run Tests](https://github.com/callan321/ignite-life-server/actions/workflows/ci.yml/badge.svg)

## 🛠️ Prerequisites (For Development)

### Windows and MacOS

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

Includes both **Docker Engine** and **Docker Compose**, so no extra installs needed. Make sure Docker Desktop is **running** before running commands.

### Linux Users

- Install [Docker Engine](https://docs.docker.com/engine/install/)  
- Install [Docker Compose](https://docs.docker.com/compose/install/) (if not bundled)

---

## 🚀 Development Setup  

To get started, follow these steps:

### Clone the repository

```bash
git clone https://github.com/callan321/ignite-life-server.git
```

```bash
cd ignite-life-server
```

### 🚀 Build and Run the Test Environment

You can run the tests in one of two ways:

---

#### 🔁 Option 1: Run tests fully inside Docker

```bash
docker compose up --build --exit-code-from tests tests
```

This command will:

- Builds the test image
- Starts a PostgreSQL test database
- Runs all tests inside Docker
- Exits with the test result (just like CI)

💡 Note: This provides the exact same experience as CI, but it’s slower to build. Recommended mainly before committing changes.

### 🧪 Option 2: Run tests locally with Docker DB

much faster and the reccomneded way for dev but not exact thing 

1. Start the test database:

    ```bash
    docker compose up -d test_db
    ```

2. Wait for PostgreSQL to be ready.
3. Run tests locally:

    ```bash
    dotnet test
    ```

💡 Note: This method is much faster and recommended for day-to-day development, but it won’t be an exact match to the CI environment.

---
