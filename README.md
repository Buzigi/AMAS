# AMAS

Advanced Medical Appointment Scheduler (Medtronic interview)

## Overview

AMAS is a web-based application designed to manage medical appointments efficiently. It provides APIs for creating, updating, retrieving, and deleting appointments while ensuring no scheduling conflicts.

## Prerequisites

- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/)

## Build and Run Instructions

### Running Locally

**Note:** Running locally for testing purposes uses In Memory DB, that is reseted with every run of the application.

1. **Clone the Repository**

   ```bash
   git clone https://github.com/Buzigi/AMAS
   cd AMAS
   ```

2. **Run the Application**

   ```bash
   dotnet run --project src/MedSched.Api/MedSched.Api.csproj
   ```

3. **Access the Application**
   - The application will be available at `http://localhost:5077`.
   - Swagger documentation is available at `http://localhost:5077/swagger`.

### Running with Docker

**Note:** Running in Docker for testing purposes uses In Memory DB, that is reseted with every run of the application.

1. **Build and Start Services**

   ```bash
   docker-compose up --build
   ```

2. **Access the Application**
   - The application will be available at `http://localhost:80`.
   - Swagger documentation is available at `http://localhost:80/swagger`.

### Running on Render.com

The application is also deployed on Render.com and can be accessed at any time at:

[https://medsched.buzigi.com](https://medsched.buzigi.com)

You can use this URL to interact with the live version of the application, including its API and Swagger documentation.
This live application uses a Render.com Postgres db that is persistant.

### Running Tests

1. **Run Unit Tests**
   ```bash
   dotnet test tests/MedSched.Api.UnitTests/MedSched.Api.UnitTests.csproj
   ```
2. **Postman**

for further testing, import file [MedSched.postman_collection.json](MedSched.postman_collection.json) into Postman.

## Project Structure

- `src/MedSched.Api/`: Contains the main API project.
- `tests/MedSched.Api.UnitTests/`: Contains unit tests for the API.
- `docker-compose.yml`: Docker configuration for the application.

## Controller Functionality

The `AppointmentsController` in the `src/MedSched.Api/Controllers/` directory handles HTTP requests related to medical appointments. It provides the following endpoints:

- **GET /api/Appointments**: Retrieves all appointments.
- **GET /api/Appointments/{id}**: Retrieves a specific appointment by its ID.
- **GET /api/Appointments/{hcName}**: Retrieves appointments for a specific healthcare professional.
- **POST /api/Appointments**: Creates a new appointment. Checks for conflicts.
- **PUT /api/Appointments/{id}**: Updates an existing appointment by its ID. Checks for conflicts.
- **DELETE /api/Appointments/{id}**: Deletes an appointment by its ID.

Each endpoint interacts with the `AppointmentService` to perform the necessary operations and returns appropriate HTTP responses.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
