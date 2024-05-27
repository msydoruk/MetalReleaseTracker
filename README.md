# MetalReleaseTracker

MetalReleaseTracker is a project designed to aggregate and display metal music releases, focusing on Ukrainian bands. It allows users to view and compare album listings from various international metal music labels.

## Getting Started

### Prerequisites

- .NET 8 SDK
- SQL Server (for production) or In-Memory database (for development/testing)

### Installing

1. Clone the repository:
    ```bash
    git clone https://github.com/yourusername/MetalReleaseTracker.git
    cd MetalReleaseTracker
    ```

2. Install dependencies:
    ```bash
    dotnet restore
    ```

3. Set up the database connection in `appsettings.json`:
    ```json
    {
      "ConnectionStrings": {
        "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MetalReleaseDb;Trusted_Connection=True;"
      }
    }
    ```

4. Run the initial migration and update the database:
    ```bash
    dotnet ef migrations add InitialCreate
    dotnet ef database update
    ```

### Running the Project

To run the project, use the following command:
```bash
dotnet run
