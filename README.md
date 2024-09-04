# ApiService

## Overview
**ApiService** is a C# worker service designed to run in the background, fetching data from a private API and storing it in a database. This project showcases my ability to build robust background services, handle API interactions, and manage database operations in C#.

### Key Features:
- Background processing using .NET Worker Service.
- API interaction and data retrieval.
- Efficient database storage.
- Configurable execution intervals.

## Note on Implementation Choices
Due to time constraints, I did not have the opportunity to learn and implement an ORM like Entity Framework in this project. Instead, I used raw SQL commands encapsulated as string variables for database interactions. While this approach works, using an ORM could simplify database management and improve maintainability.

Additionally, the current method of handling fetching intervals directly in the `ExecuteAsync` method can be enhanced. More robust approaches might include:
- Utilizing **Timers** to manage recurring tasks more efficiently.
- Implementing a **cron job** on the server to handle task scheduling externally.

**Unused Packages**: Some packages are included in the project but are not actively used in the current codebase. These were originally utilized in parts of the code that have been removed for privacy reasons.

## Configuration
### Before Deployment:
- **Connection String**: Update the `connectionString` in `App.config` or provide it via environment variables.
- **Execution Interval**: In `Worker.cs`, modify the `ExecuteAsync` method to adjust the processing interval by changing `nextCompany.NextProcessingTime` to use `AddMinutes` instead of `AddSeconds`, based on your requirement.

## Deployment Instructions

### To Start the Service on Windows:
1. **Publish the Program**:
   - Go to the publish settings.
   - Set **Deployment mode** to `Self-contained`.
   - Set **Target runtime** to `win-x86`.
   - Enable **Produce single file**.

2. **Create the Windows Service**:
   - Open `cmd` as Administrator.
   - Execute the following command:
     ```bash
     sc create ServiceName binPath= "Path\To\Program.exe"
     ```
   - Start the service with:
     ```bash
     sc start ServiceName
     ```

## Usage
Due to the private nature of the API, this service is not directly reusable for other APIs. However, it serves as a solid template for creating similar worker services that interact with APIs and databases.

## Limitations
This project is tailored to a specific private API, so direct reuse is limited.
