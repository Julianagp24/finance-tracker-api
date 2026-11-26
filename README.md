<div align="center">

Project for the Escola Superior de Tecnologia, Universidade Politécnica do Cávado e do Ave

# FinanceTrackerApi

---

**A modern, production-grade Personal Finance RESTful Web API built with ASP.NET Core (.NET 11), Entity Framework Core, SQLite, and JWT Authentication.**

<p align="center">
  <a href="https://dotnet.microsoft.com/"><img src="https://img.shields.io/badge/.NET-11.0-512BD4?style=flat&logo=dotnet&logoColor=white" alt=".NET 11" /></a>
  <a href="https://learn.microsoft.com/dotnet/csharp/"><img src="https://img.shields.io/badge/C%23-13.0-239120?style=flat&logo=csharp&logoColor=white" alt="C#" /></a>
  <a href="https://www.sqlite.org/"><img src="https://img.shields.io/badge/SQLite-3-003B57?style=flat&logo=sqlite&logoColor=white" alt="SQLite" /></a>
  <a href="https://learn.microsoft.com/ef/core/"><img src="https://img.shields.io/badge/EF%20Core-11.0-68217A?style=flat" alt="EF Core" /></a>
  <a href="https://xunit.net/"><img src="https://img.shields.io/badge/xUnit-Tests-blue?style=flat" alt="xUnit" /></a>
  <a href="https://www.docker.com/"><img src="https://img.shields.io/badge/Docker-Ready-2496ED?style=flat&logo=docker&logoColor=white" alt="Docker" /></a>
</p>

<p align="center">
  <a href="LICENSE"><img src="https://img.shields.io/badge/license-MIT-green.svg" alt="License MIT" /></a>
</p>

</div>

---

## Key Features

- **JWT Authentication & Security:** Secure user registration and login using BCrypt password hashing and HMAC-SHA256 signed JWT tokens.
- **Multi-Tenant Data Isolation:** Strict user isolation ensures users can only access, modify, or view summary metrics for their own financial records.
- **SQLite Persistence & Migrations:** Code-first database migrations using Entity Framework Core (`dotnet-ef`).
- **Real-Time Financial Summary:** Calculates total income, total expenses, net balance, and transaction count per user.
- **Filtering & Safe Pagination:** Filter transactions by type (`Income`/`Expense`), category, and date ranges (`fromDate`/`toDate`) with memory-safe pagination (`Math.Clamp`).
- **CSV Export:** Export transaction history via `/api/transactions/export/csv`, ready to be ingested by downstream Python/Pandas data visualization scripts.
- **Automated Testing Suite:** 100% test coverage for core business logic and isolation rules using xUnit with isolated in-memory databases per test.
- **Dockerized:** Multi-stage build Dockerfile for instant cross-platform deployment.
- **Interactive API Documentation:** OpenAPI specifications with embedded visual docs powered by Scalar at `/scalar/v1`.

---

## Project Architecture

```text
finance-tracker-api/
├── Dockerfile                         # Multi-stage production container build
├── .dockerignore                      # Docker build exclusions
├── FinanceTrackerApi.slnx             # .NET 11 Solution
├── README.md
│
├── src/FinanceTrackerApi/             # Main Web API Project
│   ├── Controllers/
│   │   ├── AuthController.cs          # Register & Login endpoints
│   │   └── TransactionsController.cs  # CRUD, filtering, summary & CSV export
│   ├── Data/
│   │   └── AppDbContext.cs            # EF Core DbContext with SQLite mapping
│   ├── Dtos/                          # Strongly typed request/response contracts
│   ├── Migrations/                    # EF Core schema version history
│   ├── Models/                        # Domain entities (User, Transaction, TransactionType)
│   ├── FinanceTrackerApi.http         # Automated HTTP test suite for Rider / VS Code
│   └── Program.cs                     # Middleware pipeline, DI, Auth, OpenAPI & Scalar
│
└── tests/FinanceTrackerApi.Tests/     # Automated Test Project
    └── TransactionsControllerTests.cs # Unit & integration tests with xUnit & InMemory EF
```

---

## API Endpoints

### Authentication (`/api/auth`)
| Method | Endpoint | Description | Auth Required |
| :--- | :--- | :--- | :---: |
| `POST` | `/api/auth/register` | Register a new user and receive a JWT token | No |
| `POST` | `/api/auth/login` | Authenticate with email/password and receive a JWT token | No |

### Transactions (`/api/transactions`)
| Method | Endpoint | Description | Auth Required |
| :--- | :--- | :--- | :---: |
| `GET` | `/api/transactions` | List paginated transactions with type, category, and date filters | **Yes (JWT)** |
| `GET` | `/api/transactions/{id}` | Get transaction details by ID | **Yes (JWT)** |
| `POST` | `/api/transactions` | Create a new income or expense transaction | **Yes (JWT)** |
| `PUT` | `/api/transactions/{id}` | Update an existing transaction | **Yes (JWT)** |
| `DELETE`| `/api/transactions/{id}` | Delete a transaction | **Yes (JWT)** |
| `GET` | `/api/transactions/summary`| Get user's financial overview (income, expenses, balance) | **Yes (JWT)** |
| `GET` | `/api/transactions/export/csv` | Download all user transactions as a clean CSV file | **Yes (JWT)** |

---

## Getting Started

### Prerequisites
- [.NET 11 SDK](https://dotnet.microsoft.com/)
- (Optional) [Docker](https://www.docker.com/)

### 1. Clone & Setup
```bash
git clone https://github.com/Julianagp24/finance-tracker-api.git
cd finance-tracker-api
```

### 2. Apply Database Migrations
```bash
dotnet ef database update --project src/FinanceTrackerApi
```

### 3. Run the API
```bash
dotnet run --project src/FinanceTrackerApi
```
The API will be available at:
- **API Base:** `http://localhost:5174`
- **Interactive Scalar Docs:** `http://localhost:5174/scalar/v1`

---

## Running Automated Tests

Run the full xUnit test suite with:
```bash
dotnet test
```

---

## Running with Docker

Build and run the containerized application without needing the .NET SDK installed:

```bash
# Build the Docker image
docker build -t finance-tracker-api .

# Run the container
docker run -d -p 8080:8080 --name finance-api finance-tracker-api
```
Access the containerized API at `http://localhost:8080`.

---

## Future Roadmap (Multi-Language Integration)

- **Python Microservice:** A companion Python (FastAPI + Pandas + Matplotlib) service that ingests the exported CSV data to generate automated monthly expense reports, spending trend charts, and category predictions.

---

## License
This project is licensed under the MIT License.