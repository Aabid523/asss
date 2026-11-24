/* =====================================================================
   Clinic Management System - Manual Database Setup Script
   SQL Server

   This script is an ALTERNATIVE to running EF Core Migrations.
   Use ONLY ONE of the two setup methods described in README.md:
     Option A (recommended): dotnet ef database update
     Option B (manual):      run this script in SSMS / Azure Data Studio

   If you already ran "dotnet ef database update", do NOT run this file.
   ===================================================================== */

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'ClinicManagementDB')
BEGIN
    CREATE DATABASE ClinicManagementDB;
END
GO

USE ClinicManagementDB;
GO

/* ---------------------------------------------------------------------
   ASP.NET Core Identity tables
   --------------------------------------------------------------------- */

IF OBJECT_ID('dbo.AspNetRoles', 'U') IS NULL
CREATE TABLE AspNetRoles (
    Id              NVARCHAR(450) NOT NULL PRIMARY KEY,
    Name            NVARCHAR(256) NULL,
    NormalizedName  NVARCHAR(256) NULL,
    ConcurrencyStamp NVARCHAR(MAX) NULL
);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'RoleNameIndex')
CREATE UNIQUE INDEX RoleNameIndex ON AspNetRoles (NormalizedName) WHERE NormalizedName IS NOT NULL;
GO

IF OBJECT_ID('dbo.AspNetUsers', 'U') IS NULL
CREATE TABLE AspNetUsers (
    Id                      NVARCHAR(450) NOT NULL PRIMARY KEY,
    FullName                NVARCHAR(MAX) NULL,
    UserName                NVARCHAR(256) NULL,
    NormalizedUserName      NVARCHAR(256) NULL,
    Email                   NVARCHAR(256) NULL,
    NormalizedEmail         NVARCHAR(256) NULL,
    EmailConfirmed          BIT NOT NULL DEFAULT 0,
    PasswordHash            NVARCHAR(MAX) NULL,
    SecurityStamp           NVARCHAR(MAX) NULL,
    ConcurrencyStamp        NVARCHAR(MAX) NULL,
    PhoneNumber             NVARCHAR(MAX) NULL,
    PhoneNumberConfirmed    BIT NOT NULL DEFAULT 0,
    TwoFactorEnabled        BIT NOT NULL DEFAULT 0,
    LockoutEnd              DATETIMEOFFSET NULL,
    LockoutEnabled          BIT NOT NULL DEFAULT 0,
    AccessFailedCount       INT NOT NULL DEFAULT 0
);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UserNameIndex')
CREATE UNIQUE INDEX UserNameIndex ON AspNetUsers (NormalizedUserName) WHERE NormalizedUserName IS NOT NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'EmailIndex')
CREATE INDEX EmailIndex ON AspNetUsers (NormalizedEmail);
GO

IF OBJECT_ID('dbo.AspNetUserRoles', 'U') IS NULL
CREATE TABLE AspNetUserRoles (
    UserId NVARCHAR(450) NOT NULL,
    RoleId NVARCHAR(450) NOT NULL,
    CONSTRAINT PK_AspNetUserRoles PRIMARY KEY (UserId, RoleId),
    CONSTRAINT FK_AspNetUserRoles_Users FOREIGN KEY (UserId) REFERENCES AspNetUsers (Id) ON DELETE CASCADE,
    CONSTRAINT FK_AspNetUserRoles_Roles FOREIGN KEY (RoleId) REFERENCES AspNetRoles (Id) ON DELETE CASCADE
);
GO

IF OBJECT_ID('dbo.AspNetUserClaims', 'U') IS NULL
CREATE TABLE AspNetUserClaims (
    Id          INT IDENTITY PRIMARY KEY,
    UserId      NVARCHAR(450) NOT NULL,
    ClaimType   NVARCHAR(MAX) NULL,
    ClaimValue  NVARCHAR(MAX) NULL,
    CONSTRAINT FK_AspNetUserClaims_Users FOREIGN KEY (UserId) REFERENCES AspNetUsers (Id) ON DELETE CASCADE
);
GO

IF OBJECT_ID('dbo.AspNetUserLogins', 'U') IS NULL
CREATE TABLE AspNetUserLogins (
    LoginProvider           NVARCHAR(450) NOT NULL,
    ProviderKey             NVARCHAR(450) NOT NULL,
    ProviderDisplayName     NVARCHAR(MAX) NULL,
    UserId                  NVARCHAR(450) NOT NULL,
    CONSTRAINT PK_AspNetUserLogins PRIMARY KEY (LoginProvider, ProviderKey),
    CONSTRAINT FK_AspNetUserLogins_Users FOREIGN KEY (UserId) REFERENCES AspNetUsers (Id) ON DELETE CASCADE
);
GO

IF OBJECT_ID('dbo.AspNetUserTokens', 'U') IS NULL
CREATE TABLE AspNetUserTokens (
    UserId          NVARCHAR(450) NOT NULL,
    LoginProvider   NVARCHAR(450) NOT NULL,
    Name            NVARCHAR(450) NOT NULL,
    Value           NVARCHAR(MAX) NULL,
    CONSTRAINT PK_AspNetUserTokens PRIMARY KEY (UserId, LoginProvider, Name),
    CONSTRAINT FK_AspNetUserTokens_Users FOREIGN KEY (UserId) REFERENCES AspNetUsers (Id) ON DELETE CASCADE
);
GO

IF OBJECT_ID('dbo.AspNetRoleClaims', 'U') IS NULL
CREATE TABLE AspNetRoleClaims (
    Id          INT IDENTITY PRIMARY KEY,
    RoleId      NVARCHAR(450) NOT NULL,
    ClaimType   NVARCHAR(MAX) NULL,
    ClaimValue  NVARCHAR(MAX) NULL,
    CONSTRAINT FK_AspNetRoleClaims_Roles FOREIGN KEY (RoleId) REFERENCES AspNetRoles (Id) ON DELETE CASCADE
);
GO

/* ---------------------------------------------------------------------
   Business tables
   --------------------------------------------------------------------- */

IF OBJECT_ID('dbo.Specialties', 'U') IS NULL
CREATE TABLE Specialties (
    Id          INT IDENTITY PRIMARY KEY,
    Name        NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    CONSTRAINT UQ_Specialties_Name UNIQUE (Name)
);
GO

IF OBJECT_ID('dbo.Doctors', 'U') IS NULL
CREATE TABLE Doctors (
    Id              INT IDENTITY PRIMARY KEY,
    FullName        NVARCHAR(100) NOT NULL,
    PhoneNumber     NVARCHAR(20) NOT NULL,
    Email           NVARCHAR(150) NOT NULL,
    SpecialtyId     INT NOT NULL,
    CONSTRAINT UQ_Doctors_Email UNIQUE (Email),
    CONSTRAINT FK_Doctors_Specialties FOREIGN KEY (SpecialtyId) REFERENCES Specialties (Id) ON DELETE NO ACTION
);
GO

IF OBJECT_ID('dbo.Patients', 'U') IS NULL
CREATE TABLE Patients (
    Id              INT IDENTITY PRIMARY KEY,
    FullName        NVARCHAR(100) NOT NULL,
    DateOfBirth     DATETIME2 NOT NULL,
    Gender          NVARCHAR(10) NOT NULL,
    PhoneNumber     NVARCHAR(20) NOT NULL,
    Email           NVARCHAR(150) NULL,
    Address         NVARCHAR(250) NULL
);
GO

IF OBJECT_ID('dbo.Appointments', 'U') IS NULL
CREATE TABLE Appointments (
    Id                  INT IDENTITY PRIMARY KEY,
    PatientId           INT NOT NULL,
    DoctorId            INT NOT NULL,
    AppointmentDate     DATETIME2 NOT NULL,
    DurationMinutes     INT NOT NULL DEFAULT 30,
    Status              NVARCHAR(20) NOT NULL DEFAULT 'Scheduled',
    Notes               NVARCHAR(500) NULL,
    CONSTRAINT FK_Appointments_Patients FOREIGN KEY (PatientId) REFERENCES Patients (Id) ON DELETE CASCADE,
    CONSTRAINT FK_Appointments_Doctors FOREIGN KEY (DoctorId) REFERENCES Doctors (Id) ON DELETE NO ACTION
);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Appointments_Doctor_Date')
CREATE INDEX IX_Appointments_Doctor_Date ON Appointments (DoctorId, AppointmentDate);
GO

IF OBJECT_ID('dbo.Visits', 'U') IS NULL
CREATE TABLE Visits (
    Id              INT IDENTITY PRIMARY KEY,
    AppointmentId   INT NULL,
    PatientId       INT NOT NULL,
    DoctorId        INT NOT NULL,
    VisitDate       DATETIME2 NOT NULL,
    Diagnosis       NVARCHAR(300) NULL,
    Treatment       NVARCHAR(300) NULL,
    Notes           NVARCHAR(500) NULL,
    CONSTRAINT UQ_Visits_AppointmentId UNIQUE (AppointmentId),
    CONSTRAINT FK_Visits_Appointments FOREIGN KEY (AppointmentId) REFERENCES Appointments (Id) ON DELETE SET NULL,
    CONSTRAINT FK_Visits_Patients FOREIGN KEY (PatientId) REFERENCES Patients (Id) ON DELETE CASCADE,
    CONSTRAINT FK_Visits_Doctors FOREIGN KEY (DoctorId) REFERENCES Doctors (Id) ON DELETE NO ACTION
);
GO

/* ---------------------------------------------------------------------
   Seed: roles + default admin + sample lookup data
   NOTE: the admin password hash below corresponds to "Admin@12345" ONLY
   when the app's seeder (DbInitializer) has not already run. In practice,
   simply run the application once (Option A) and let DbInitializer create
   the admin account correctly through ASP.NET Identity's own hasher.
   If you set up the database manually via this script, start the app
   afterwards - DbInitializer will detect there is no admin account yet
   (by email) and will create one automatically.
   --------------------------------------------------------------------- */

IF NOT EXISTS (SELECT 1 FROM Specialties)
BEGIN
    INSERT INTO Specialties (Name, Description) VALUES
    (N'General Medicine', N'Primary care and general checkups.'),
    (N'Dentistry', N'Dental care and oral health.'),
    (N'Pediatrics', N'Medical care for infants, children and adolescents.'),
    (N'Dermatology', N'Skin, hair and nail conditions.');
END
GO

PRINT 'Schema created successfully. Start the application to let it seed roles, the default admin account and a sample doctor.';
