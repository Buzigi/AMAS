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

3. **Frontend - Web Page**

DEMO

```
cd frontend/
npm run dev
```

## Project Structure

- `src/MedSched.Api/`: Contains the main API project.
- `tests/MedSched.Api.UnitTests/`: Contains unit tests for the API.
- `frontend/` : contains all the files for the client demo application.
- `docker-compose.yml`: Docker configuration for the application.

## Controller Functionality

The `AppointmentsController` in the `src/MedSched.Api/Controllers/` directory handles HTTP requests related to medical appointments. It provides the following endpoints that interacts with the `AppointmentService` to perform the necessary operations and returns appropriate HTTP responses:

### - **GET /api/Appointments**:

Retrieves all appointments.

#### Request:

```
http://localhost:5077/api/appointments
```

#### Response:

```
{
   {
      "id": 1,
      "patientName": "Boaz",
      "healthcareProfessionalName": "Dr. Smith",
      "appointmentDate": "2025-05-24T08:00:00Z",
      "duration": 30,
      "description": "Ear infection check-up"
   },
   {
      "id": 2,
      "patientName": "Alice",
      "healthcareProfessionalName": "Dr. Johnson",
      "appointmentDate": "2025-05-24T08:30:00Z",
      "duration": 45,
      "description": "Routine dental cleaning"
   }
}
```

### - **GET /api/Appointments/{id}**:

Retrieves a specific appointment by its ID.

#### Request:

```
http://localhost:5077/api/appointments/7
```

#### Response:

```
{
    "id": 7,
    "patientName": "George",
    "healthcareProfessionalName": "Dr. Smith",
    "appointmentDate": "2025-05-24T11:35:00Z",
    "duration": 45,
    "description": "Annual physical exam"
}
```

### - **GET /api/Appointments/{hcName}**:

Retrieves appointments for a specific healthcare professional.

#### Request:

```
http://localhost:5077/api/appointments/Dr. Johnson
```

#### Response:

```
[
    {
        "id": 2,
        "patientName": "Alice",
        "healthcareProfessionalName": "Dr. Johnson",
        "appointmentDate": "2025-05-24T08:30:00Z",
        "duration": 45,
        "description": "Routine dental cleaning"
    },
    {
        "id": 5,
        "patientName": "Eli",
        "healthcareProfessionalName": "Dr. Johnson",
        "appointmentDate": "2025-05-24T10:45:00Z",
        "duration": 20,
        "description": "Blood pressure check"
    }
]
```

### - **POST /api/Appointments**:

Creates a new appointment. Checks for conflicts.

#### Request:

```
http://localhost:5077/api/appointments/
```

with body

```
{
   "patientName": "Boaz",
   "healthcareProfessionalName": "Dr. Johnson",
   "appointmentDate": "2025-05-24T08:30:24.4Z",
   "duration": 30,
   "description": "Ear infection"
}
```

#### Response:

If there are no time conflicts:

```
{
    "success": true,
    "suggestedTimes": []
}
```

If there are time conflicts:

```
{
    "success": false,
    "suggestedTimes": [
        {
            "appointmentStart": "2025-05-24T09:15:00Z",
            "duration": 30
        },
        {
            "appointmentStart": "2025-05-24T09:45:00Z",
            "duration": 30
        },
        {
            "appointmentStart": "2025-05-24T10:15:00Z",
            "duration": 30
        },
        {
            "appointmentStart": "2025-05-24T11:05:00Z",
            "duration": 30
        }
    ]
}
```

### - **PUT /api/Appointments/{id}**:

Updates an existing appointment by its ID. Checks for conflicts.

#### Request:

```
http://localhost:5077/api/appointments/1
```

with body

```
{
   "description": "Testing",
   "appointmentDate": "2025-05-03T08:30:24.4Z"
}
```

#### Response:

If there are no time conflicts, an empty 200 success code.

If there are time conflicts:

```
{
    "success": false,
    "suggestedTimes": [
        {
            "appointmentStart": "2025-05-24T11:05:00Z",
            "duration": 45
        },
        {
            "appointmentStart": "2025-05-24T12:50:00Z",
            "duration": 45
        },
        {
            "appointmentStart": "2025-05-24T13:35:00Z",
            "duration": 45
        },
        {
            "appointmentStart": "2025-05-24T15:05:00Z",
            "duration": 45
        }
    ]
}
```

### - **DELETE /api/Appointments/{id}**:

Deletes an appointment by its ID.

#### Request:

```
http://localhost:5077/api/appointments/20
```

#### Response:

An empty 200 code.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
