# SchoolManagementSystem — Agents Guide

## Project structure (Clean Architecture)

```
SchoolManagementSystem.sln / .slnx    — two solution files; .slnx is newer, includes Assets
├── Presentation/
│   ├── SchoolManagement/              — main WPF app (WinExe, net10.0-windows)
│   ├── AttendanceScanner/             — QR scanner WPF app (separate WinExe)
│   ├── CandidateManagement/           — enrollment WPF app (separate WinExe)
│   └── Shared/                        — shared WPF lib: views, services, DI registration
├── Application/                       — use cases, services, auth handlers, report generators
├── Infrastructure/                    — EF Core DbContext, repositories, S3, report exporters
├── Core/                              — domain models, interfaces, shared config
│   └── School_Management.Core/        — ORPHAN (underscore variant, only obj/ remains)
├── Assets/                            — images, fonts, spreadsheets (multi-targets net10.0;net10.0-windows)
└── Tests/
    ├── SchoolManagement.Tests/        — xUnit + Moq tests for Application/Infra services
    ├── AttendanceScanner.UnitTests/   — xUnit + Moq tests for AttendanceScanner
    ├── TestConsole/                   — ad-hoc DB scripts (not a real test project)
    └── DataMigrator/                  — MS Access → PostgreSQL migrator (legacy, references old underscore namespace)
```

## Framework & toolchain

- **.NET 10.0** (`net10.0` / `net10.0-windows`), C# 13, `Nullable: enable`, `ImplicitUsings: enable`
- **WPF** with `<UseWPF>true</UseWPF>` in all presentation projects
- **EF Core 10.0.7** + **Npgsql 10.0.1** (PostgreSQL), connection pooling enabled, retry on failure (3x)
- **CommunityToolkit.Mvvm 8.4.2** (source generators: `[ObservableProperty]`, `[RelayCommand]`)
- **MaterialDesignThemes 5.3.2** (Material Design 3)
- **LiveChartsCore 2.0.2** (SkiaSharp-backed WPF charts)
- **ClosedXML 0.105.0** (Excel export), **QuestPDF 2026.5.0** (PDF export)
- **ZXing.Net 0.16.11** + Bindings (QR codes)
- **AForge.Video 2.2.5** (camera capture)
- **AWSSDK.S3 4.0.23** (photo storage)
- **BCrypt.Net-Core 1.6.0** (password hashing)
- **FastCloner 3.5.4** (deep clone utility)
- **KhmerCalendar 1.0.0** (Khmer calendar)
- **MahApps.Metro 2.4.11** (used alongside MaterialDesignThemes in Shared)
- **No Directory.Build.props**, **no nuget.config**, **no global.json**

## Database

- PostgreSQL, connection read from `DB_CONNECTION` env var (loaded from `Infrastructure/SchoolManagement.Infrastructure/.env` via `SecretHelper.GetValueFromEnv()`)
- `.env` is gitignored, contains `DB_CONNECTION`, `ACCESS_KEY`, `SECRET_KEY`
- 28 `DbSet<>` properties in `SchoolDbContext`, 31 EF Core migrations
- Use `Attendance` enum stored as string via `.HasConversion<string>()`
- EF CLI tool (local): `dotnet ef` (v10.0.8, registered in `dotnet-tools.json` at root)
- Migration commands (run from Infrastructure project directory):
  - `dotnet ef migrations add <Name>` (scoped to Infrastructure project)
  - `dotnet ef database update`

## Solution files

- `SchoolManagementSystem.sln` — VS 2022 legacy format (9 projects, missing Assets)
- `SchoolManagementSystem.slnx` — newer XML format (10 projects, includes Assets)
- Prefer `.slnx` for builds; both are kept in sync

## Build & test commands

```powershell
dotnet build SchoolManagementSystem.slnx
dotnet test Tests\SchoolManagement.Tests\SchoolManagement.Tests.csproj
dotnet test Tests\AttendanceScanner.UnitTests\AttendanceScanner.UnitTests.csproj
dotnet test SchoolManagementSystem.slnx            # all test projects
```

- NOTE: You cannot build the solution if you are using linux

- Test framework: **xUnit** (v2.9.3), **Moq** (v4.20.72), **coverlet** (v10.0.0)
- Two test projects:
  - `SchoolManagement.Tests` targets `net10.0`, references Application + Core + Infrastructure  
  - `AttendanceScanner.UnitTests` targets `net10.0-windows`, references AttendanceScanner project

## Architecture patterns

- **MVVM**: ViewModels use `CommunityToolkit.Mvvm` source generators, Views use WPF XAML data templates (mapped in `App.xaml`)
- **Navigation**: `INavigationService` (singleton), views implement `IAsyncLoadable` / `INavigationAware`
- **DI**: All 4 layers expose `Add{Layer}()` extension methods on `IServiceCollection`; main WPF app uses `Microsoft.Extensions.Hosting` (AttendaceScanner uses raw `ServiceCollection`)
- **Auth**: 9 `IAuthorizationHandler` implementations (Attendance, Student, Class, Employee, Candidate, Grade, Exam, Score, AuditLog), registered in Application layer
- **Reports**: 4 `IReportGenerator` implementations (StudentRoster, Attendance, Score, StudentCard), `IReportExporter` implementations (Excel, Pdf), `IReportViewProvider` pattern
- **Photos**: Student/Employee photos synced to S3 via `IPhotoSyncService`, with `IPhotoUploadService` / `IPhotoFetchService` / `IPhotoDeleteService` in Application layer

## Styles & conventions

- File-scoped namespaces throughout
- `GlobalUsings.cs` in every project
- `.editorconfig` suppresses CA2201 (reserved exception types)
- `[Fact]` only (no `[Theory]` seen in tests)

## Known quirks & gotchas

- `IReportComponentFactory` is registered but **superseded** by the provider pattern — don't rely on it
- `AttendanceScanner.csproj` in `.slnx` uses `Attendance_Scanner.csproj` (underscore) in its test project reference — if it breaks, check the path
- `Tests/DataMigrator` references `School_Management.Application` (old underscore namespace) — the real namespace is `SchoolManagement.Application`
- `Core/School_Management.Core/` (underscore variant) is an **orphan** — only `obj/` remains, not in solution, not referenced by anything
- No CI/CD, no Docker, no scripts — all manual builds
- Credentials exist in `.env` (gitignored), `TestConsole/Program.cs`, and `DataMigrator/Program.cs`

## Reports for more detail

- `DOCUMENTATION.md` — overall architecture, feature creation guide, DB setup, build/run steps
- `docs/REPORT_FEATURE.md` — report system deep dive
- `Application/.../Policies/Docs/00_START_HERE.md` — authorization system reference
