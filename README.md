# Clinic Management System

A full-stack clinic management system built with **ASP.NET Core MVC (.NET 8)**,
**Entity Framework Core**, and **SQL Server**.

## Features

- Role-based login (Admin / Receptionist / Doctor) via ASP.NET Core Identity
- Patient records (CRUD), with full appointment and visit history
- Doctor and Specialty management
- Interactive appointment calendar (FullCalendar.js) with double-booking prevention
- Visit / consultation records linked to appointments and patients
- Dashboard with live stats and charts (Chart.js)
- Search and filtering on every list page
- Clean, responsive Bootstrap 5 interface

No payment, billing or invoicing features are included, by design.

## Tech Stack

| Layer      | Technology                                   |
|------------|-----------------------------------------------|
| Backend    | ASP.NET Core MVC (.NET 8), C#                 |
| Data       | Entity Framework Core 8, SQL Server           |
| Auth       | ASP.NET Core Identity (cookie-based, roles)   |
| Frontend   | Bootstrap 5, Bootstrap Icons, vanilla JS       |
| Charts     | Chart.js                                       |
| Calendar   | FullCalendar.js                                |

## Project Structure

```
ClinicManagementSystem/
├── ClinicManagementSystem.sln
├── Database/
│   └── Schema.sql                 <- manual DB setup script (optional, see below)
└── ClinicManagementSystem/
    ├── Controllers/
    ├── Data/                      <- ApplicationDbContext + DbInitializer (seeding)
    ├── Models/                    <- Patient, Doctor, Specialty, Appointment, Visit, ApplicationUser
    ├── ViewModels/
    ├── Views/
    ├── wwwroot/                   <- site.css, site.js
    ├── appsettings.json           <- connection string lives here
    └── Program.cs
```

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (Express, Developer, or full edition) **or** a SQL Server LocalDB instance
- (Optional) Visual Studio 2022 17.8+ or VS Code with the C# extension

## How to Run (step by step)

### 1. Open the project
Open `ClinicManagementSystem.sln` in Visual Studio, **or** open the root folder in VS Code / your terminal.

### 2. Set your connection string
Edit `ClinicManagementSystem/appsettings.json` and update `ConnectionStrings:DefaultConnection`
to point at your SQL Server instance. The default value works for a local
`SQLEXPRESS` instance with Windows auth:

```json
"DefaultConnection": "Server=.\\SQLEXPRESS;Database=ClinicManagementDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
```

### 3. Restore packages
```bash
cd ClinicManagementSystem
dotnet restore
```

### 4. Set up the database — choose ONE option

**Option A — EF Core Migrations (recommended)**

This generates and applies migrations for you, creating *all* tables
(Identity + business tables) in one step.

```bash
dotnet tool install --global dotnet-ef     # only needed once per machine
dotnet ef migrations add InitialCreate
dotnet ef database update
```

The app also calls `Database.MigrateAsync()` automatically on startup, so once
a migration exists, simply running the app will keep the database up to date.

**Option B — Manual SQL script**

If you prefer not to use the EF CLI tools, run `Database/Schema.sql` against
your SQL Server instance (e.g. in SSMS or Azure Data Studio). This creates
the database and every table by hand. Do **not** also run EF migrations
against the same database afterwards — pick one option, not both.

### 5. Run the application
```bash
dotnet run
```
Then open the URL shown in the console (e.g. `https://localhost:7xxx`).

On first run, the app automatically seeds:
- The three roles: `Admin`, `Receptionist`, `Doctor`
- A default admin account
- Sample specialties and one sample doctor

### Default login

| Email              | Password      | Role  |
|--------------------|---------------|-------|
| admin@clinic.com   | Admin@12345   | Admin |

Log in as Admin, then use **Add Staff** in the sidebar to create Receptionist
and Doctor accounts for your team. Remember to also add a matching **Doctor**
record (Doctors → New Doctor) for anyone who needs to appear in appointment
dropdowns.

## Roles & Permissions

| Area           | Admin | Receptionist | Doctor |
|----------------|:-----:|:------------:|:------:|
| Dashboard      |   ✔   |      ✔       |   ✔    |
| Patients       |   ✔   |      ✔       |   —    |
| Appointments   |   ✔   |      ✔       | view only |
| Visit Records  |   ✔   |      —       |   ✔    |
| Doctors        |   ✔   |  view only   |   —    |
| Specialties    |   ✔   |  view only   |   —    |
| Add Staff      |   ✔   |      —       |   —    |

## Notes

- Appointment booking blocks overlapping time slots for the same doctor.
- Saving a Visit record linked to an appointment automatically marks that
  appointment as **Completed**.
- This build intentionally excludes billing/invoicing per project scope.
