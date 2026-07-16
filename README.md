# School Management System

A comprehensive school management system built with .NET 10, WPF, and Clean Architecture. Designed for Cambodian schools with full Khmer and English bilingual support, covering student enrollment, attendance tracking, grading, reporting, and administrative operations.

---

## Table of Contents

- [Features](#features)
- [Architecture](#architecture)
- [Project Structure](#project-structure)
- [Tech Stack](#tech-stack)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [Authentication & Authorization](#authentication--authorization)
- [Testing](#testing)
- [Bilingual Support](#bilingual-support)

---

## Features

### Student & Candidate Management
- Full student CRUD with enrollment from candidates
- Candidate enrollment application with spreadsheet import (MS Access / Excel)
- Student-to-class assignment with active/inactive tracking
- Photo management with AWS S3 storage and local caching
- QR code generation per student for attendance scanning

### Class & Department Management
- Class creation by grade, generation, and department
- Subject assignment to classes with teacher assignment
- Student roster management per class
- Academic departments, grades, generations (cohorts), and skills

### Attendance Tracking
- Manual attendance entry via wizard-based workflow
- QR code scanning via dedicated camera application
- Six attendance statuses: Present, Absent, Late, Excused, ExcusedLate, EarlyLeave
- Duplicate scan prevention and timestamp tracking

### Grading & Scoring
- Component-based assessment system (e.g., midterm, final, assignments)
- Subject-component mapping for flexible grading schemes
- Exam management with per-student scoring
- Score records linked to assessments, subjects, and exams

### Employee & Staff Management
- Teacher and staff CRUD with department assignment
- Salary tracking (base salary, bonus, deduction, tax)
- User account creation linked to employee records
- Employee photo management via S3

### Reporting
- Four report types: Student Roster, Attendance Report, Score Report, Student Name Cards
- Live preview with table and card-style layouts
- PDF export via QuestPDF
- Excel export via ClosedXML
- Filter-based report generation with Khmer calendar support

### Administration
- Role-based access control (Admin, HeadTeacher, Teacher, HeadMaster)
- 16 granular permissions across all modules
- 11 authorization handlers with department/class-level scoping
- Automatic audit logging via EF Core interceptor
- Role and permission management UI
- Dashboard with LiveChartsCore-powered statistics

### Infrastructure
- AWS S3 photo storage with background sync worker
- BCrypt password hashing with account lockout
- PostgreSQL database with 79 EF Core migrations
- Automatic audit trail capturing old/new values on all mutations

---

## Architecture

The system follows **Clean Architecture** with strict dependency rules:

```
┌─────────────────────────────────────────────────────┐
│                  Presentation (WPF)                  │
│  SchoolManagement · AttendanceScanner · CandidateMgmt│
├─────────────────────────────────────────────────────┤
│                   Application Layer                  │
│  Services · Authorization · Reports · Validators     │
├─────────────────────────────────────────────────────┤
│                 Infrastructure Layer                 │
│  EF Core · PostgreSQL · AWS S3 · PDF/Excel Export    │
├─────────────────────────────────────────────────────┤
│                     Core Layer                       │
│  Entities · Enums · Interfaces · Contracts           │
└─────────────────────────────────────────────────────┘
```

**Dependency rule**: Each layer only depends on the layer directly below it. Core has zero external dependencies.

**Patterns used**:
- **MVVM** with CommunityToolkit.Mvvm source generators (`[ObservableProperty]`, `[RelayCommand]`)
- **Generic Host** (`Microsoft.Extensions.Hosting`) for dependency injection
- **Repository Pattern** (22+ repositories)
- **Service Layer** (28+ application services)
- **Authorization Handler Chain** (11 handlers with chain-of-responsibility)
- **Factory Pattern** for report generation
- **Interceptor Pattern** for automatic audit logging

---

## Project Structure

```
SchoolManagementSystem/
├── Core/
│   └── SchoolManagement.Core/            # Domain entities, enums, interfaces (zero dependencies)
├── Application/
│   └── SchoolManagement.Application/     # Business logic, services, authorization, reports
├── Infrastructure/
│   └── SchoolManagement.Infrastructure/  # EF Core, PostgreSQL, S3, PDF/Excel export
├── Presentation/
│   ├── SchoolManagement/                 # Main WPF application (38 views)
│   ├── AttendanceScanner/                # QR code attendance scanner app
│   ├── CandidateManagement/              # Candidate enrollment app
│   └── Shared/                           # Shared WPF services, converters, themes
├── Assets/                               # Shared assets (fonts, images)
├── Tests/
│   ├── SchoolManagement.Tests/           # Unit tests (xUnit + Moq)
│   ├── AttendanceScanner.UnitTests/      # Scanner-specific tests
│   ├── DataMigrator/                     # Legacy MS Access migration tool
│   └── TestConsole/                      # Utility console app
├── SchoolManagementSystem.sln            # Visual Studio solution (9 projects)
└── SchoolManagementSystem.slnx           # Extended solution (10 projects, includes Assets)
```

### Key Domain Entities (25+)

| Entity | Description |
|--------|-------------|
| `Candidate` | Enrollment candidate with Khmer + Latin names, personal info, exam details |
| `Student` | Enrolled student (delegates personal info to Candidate) |
| `StudentClass` | Many-to-many enrollment between Student and Class |
| `StudentQR` | QR code reference for attendance scanning |
| `Class` | Academic class (Grade + Generation + Department) |
| `ClassSubject` | Subject assignment to a class with teacher |
| `Employee` | Teacher/staff with salary info and department assignment |
| `Department` | Academic department |
| `Generation` | Academic cohort within a department |
| `Grade` | Academic grade level |
| `Skill` | Skill/specialty (1:1 with Department) |
| `Subject` | Academic subject with max score |
| `SubjectComponent` | Assessment component (e.g., midterm, final) |
| `SubjectMapper` | Junction linking Subject to SubjectComponent |
| `Assessment` | Student's assessment record per subject+exam |
| `Score` | Individual score for a subject component |
| `Exam` | Exam definition |
| `Attendance` | Attendance record with status and timestamps |
| `User` | Authentication user with lockout tracking |
| `Role` | User role with assigned permissions |
| `Permission` | Granular permission definition |
| `Notification` | Student notification |
| `AuditLog` | Automatic audit trail for all mutations |

---

## Tech Stack

### Core
- .NET 10.0 / C# 13
- Nullable reference types enabled
- Implicit usings enabled

### Backend
| Technology | Purpose |
|-----------|---------|
| Entity Framework Core 10.0.8 | ORM |
| PostgreSQL (Npgsql 10.0.2) | Database |
| BCrypt.Net-Core 1.6.0 | Password hashing |
| QuestPDF 2026.5.0 | PDF generation |
| ClosedXML 0.105.0 | Excel generation |
| AWSSDK.S3 4.0.24 | Photo storage |
| ZXing.Net 0.16.11 | QR code generation |
| SkiaSharp 3.119.4 | Graphics rendering |
| DotNetEnv 3.2.0 | Environment variable loading |
| KhmerCalendar 1.0.0 | Khmer calendar support |

### Frontend (WPF)
| Technology | Purpose |
|-----------|---------|
| CommunityToolkit.Mvvm 8.4.2 | MVVM framework |
| MaterialDesignThemes 5.3.2 | Material Design 3 UI |
| MahApps.Metro 2.4.11 | Metro-style window chrome |
| LiveChartsCore 2.0.4 | Dashboard charts |
| AForge.Video 2.2.5 | Camera capture (scanner) |
| ZXing.Net.Bindings 0.16.14 | QR code scanning (scanner) |

### Testing
| Technology | Purpose |
|-----------|---------|
| xUnit 2.9.3 | Test framework |
| Moq 4.20.72 | Mocking |
| coverlet.collector 10.0.1 | Code coverage |

---

## Prerequisites

- **.NET 10 SDK** ([download](https://dotnet.microsoft.com/download/dotnet/10.0))
- **PostgreSQL** server (14+ recommended)
- **Windows OS** (WPF is Windows-only)
- **AWS S3 bucket** (for photo storage, optional for development)
- **Camera** (optional, for QR attendance scanner)

---

## Getting Started

### 1. Clone the repository

```bash
git clone <repository-url>
cd SchoolManagementSystem
```

### 2. Configure environment

Create a `.env` file in `Infrastructure/SchoolManagement.Infrastructure/`:

```env
DB_CONNECTION=Host=localhost;Port=5432;Database=school_management;Username=postgres;Password=yourpassword
ACCESS_KEY=your-aws-access-key
SECRET_KEY=your-aws-secret-key
```

### 3. Set up the database

```bash
cd Infrastructure/SchoolManagement.Infrastructure
dotnet ef database update
```

### 4. Build the solution

```bash
dotnet build SchoolManagementSystem.slnx
```

### 5. Run the applications

```bash
# Main application
dotnet run --project Presentation/SchoolManagement/SchoolManagement.Presentation.csproj

# Attendance scanner (requires camera)
dotnet run --project Presentation/AttendanceScanner/AttendanceScanner.csproj

# Candidate management
dotnet run --project Presentation/CandidateManagement/CandidateManagement.csproj
```

---

## Configuration

| Source | Settings | Location |
|--------|----------|----------|
| `.env` | `DB_CONNECTION`, `ACCESS_KEY`, `SECRET_KEY`, `ONLINE_DB_CONNECTION` | `Infrastructure/SchoolManagement.Infrastructure/.env` |
| `appsettings.json` | Theme, storage paths, S3 bucket config | `Infrastructure/SchoolManagement.Infrastructure/appsettings.json` |
| User Secrets | Application and Infrastructure secrets | Via `<UserSecretsId>` in .csproj |

### Settings loaded at runtime
- **Database connection**: `SecretHelper.GetValueFromEnv("DB_CONNECTION")`
- **AWS S3 credentials**: `ACCESS_KEY` and `SECRET_KEY` from `.env`
- **S3 bucket**: `school-management-system` with paths `photos/students` and `photos/employees`
- **QuestPDF license**: Community license (set in `PdfExporter.cs`)

---

## Authentication & Authorization

### Authentication
- BCrypt password hashing
- Account lockout after failed login attempts
- Session-based user tracking via `UserSessionService`
- Login dialog before main window loads

### Roles

| Role | Access Level |
|------|-------------|
| **Admin** | Full access to all resources (fast-path bypass) |
| **HeadTeacher** | Department-scoped access |
| **Teacher** | Class-scoped access |
| **HeadMaster** | Defined but limited usage |

### Permissions (16 total)

| Module | Permissions |
|--------|------------|
| Students | View, Insert, Edit, Delete |
| Classes | View, Insert, Edit, Delete |
| Employees | Manage |
| Attendance | Manage |
| Candidates | Manage |
| Departments | Manage |
| Assessments | Manage |
| Subjects | Manage |
| Users | Manage |
| Roles | Manage |

### Access Matrix

| Resource | Admin | HeadTeacher | Teacher |
|----------|-------|-------------|---------|
| Students | Full | Department scope | Class scope |
| Attendance | Full | Department scope | Class scope |
| Classes | Full | Department scope | View only |
| Employees | Full | View department | Denied |
| Candidates | Full | Denied | Denied |
| Grades/Exams | Full | View | View |
| Scores | Full | Department scope | Class scope |

---

## Testing

### Run all tests

```bash
dotnet test SchoolManagementSystem.slnx
```

### Run specific test projects

```bash
dotnet test Tests/SchoolManagement.Tests/SchoolManagement.Tests.csproj
dotnet test Tests/AttendanceScanner.UnitTests/AttendanceScanner.UnitTests.csproj
```

### Test coverage

**SchoolManagement.Tests**:
- `CandidateTests` - Deep copy behavior for Candidate entities
- `UserValidationServiceTest` - Login validation, credential checking, account lockout
- `StudentCardGeneratorTests` - Report generation
- `PdfExporterTests` - PDF export functionality
- `ExcelExporterTests` - Excel export
- `FontResourceResolverTests` - Font resolution for reports
- `PhotoDeleteServiceTests` / `PhotoUploadServiceTests` / `PhotoFetchServiceTests` - Photo management

**AttendanceScanner.UnitTests**:
- `AttendanceQRService` - QR code validation, duplicate scan prevention, attendance creation

---

## Bilingual Support

The application supports **Khmer and English** throughout:

- Entity models include `[Description]` attributes with Khmer translations
- UI labels use `[Description]` attribute values for Khmer display
- Khmer calendar integration via `KhmerCalendar` package
- Computed columns for Khmer names (e.g., `Candidate.FullName`, `Class.KhmerName`)
- Report generators produce output with bilingual headers and labels

---

## Database

- **PostgreSQL** with Entity Framework Core
- **24 DbSets** covering all domain entities
- **79 migrations** tracking schema evolution
- **Computed columns**: `Candidate.FullName`, `Candidate.LatinFullName`
- **Enum-to-string storage**: Attendance status, gender, marital status, stay type
- **Connection pooling** with retry on failure (3 attempts)
- Default schema: `public`

---

## Report System

Four report types with dedicated generators:

| Report | Generator | Export Formats |
|--------|-----------|---------------|
| Student Roster | `StudentRosterGenerator` | PDF, Excel |
| Attendance Report | `AttendanceReportGenerator` | PDF, Excel |
| Score Report | `ScoreReportGenerator` | PDF, Excel |
| Student Name Cards | `StudentCardGenerator` | PDF (card layout) |

Reports use filter models (e.g., `StudentRosterFilter`, `AttendanceReportFilter`) and support live preview before export.

---

## Utilities

### DataMigrator
Console application for migrating legacy MS Access database data to PostgreSQL.

### script.ps1
PowerShell script for installing NuGet packages to the Presentation project.

---

## License

Specify your license here.
