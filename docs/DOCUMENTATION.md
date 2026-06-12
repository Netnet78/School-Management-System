# School Management System — Documentation

## Table of Contents

1. [Project Overview](#1-project-overview)
2. [Tech Stack](#2-tech-stack)
3. [Architecture](#3-architecture-clean-architecture)
4. [Project Structure](#4-project-structure)
5. [How the Application Starts](#5-how-the-application-starts)
6. [How Navigation Works](#6-how-navigation-works)
7. [How the Report Feature Works](#7-how-the-report-feature-works)
8. [Key Patterns](#8-key-patterns)
9. [Creating a New Feature](#9-creating-a-new-feature)
10. [Database](#10-database)
11. [Configuration](#11-configuration)
12. [Build and Run](#12-build-and-run)

---

## 1. Project Overview

This is a **School Management System** built with **WPF (.NET 10)** following **Clean Architecture** principles. It manages students, classes, attendance, exams, scores, employees, departments, and generates various reports (PDF/Excel).

There are **three separate WPF applications** in this solution:

| App | Description |
|-----|-------------|
| **SchoolManagement** (main) | Full-featured management app with dashboard, CRUD for all entities, and reporting |
| **AttendanceScanner** | QR-code-based attendance tracking using a camera |
| **CandidateManagement** | Candidate enrollment and management |

This document focuses on the **main SchoolManagement** app, but the patterns are shared across all three.

---

## 2. Tech Stack

### Framework & Language

| Technology | Version |
|------------|---------|
| .NET SDK | 10.0 |
| C# | 13 |
| WPF | net10.0-windows |

### NuGet Packages (by layer)

**Core Layer:**
- `BCrypt.Net-Core` 1.6.0 — password hashing
- `DotNetEnv` 3.2.0 — `.env` file loading
- `FastCloner` 3.5.4 — deep object cloning

**Infrastructure Layer:**
- `Npgsql.EntityFrameworkCore.PostgreSQL` 10.0.1 — PostgreSQL EF Core provider
- `QuestPDF` 2026.5.0 — PDF generation (Community license)
- `ClosedXML` 0.105.0 — Excel spreadsheet generation
- `AWSSDK.S3` 4.0.23 — AWS S3 photo storage
- `ZXing.Net` 0.16.11 — QR code generation/reading

**Presentation Layers:**
- `CommunityToolkit.Mvvm` 8.4.2 — MVVM framework (`ObservableObject`, `RelayCommand`, `[ObservableProperty]`)
- `MaterialDesignThemes` 5.3.2 — Material Design 3 UI components (MahApps.Metro 2.4.11 also present)
- `LiveChartsCore` 2.0.2 — charts and graphs
- `Microsoft.Extensions.DependencyInjection` 10.0.7 — DI container
- `Microsoft.Extensions.Hosting` 10.0.7 — Generic Host for DI, lifecycle
- `AForge.Video` 2.2.5 — camera capture
- `ZXing.Net.Bindings.Windows.Compatibility` 0.16.14 — QR scanning

### External Services

- **Database**: PostgreSQL (via EF Core)
- **Object Storage**: AWS S3 (student/employee photos)
- **Legacy Data**: Microsoft Access (migrated via DataMigrator console app)

### Testing

- xUnit 2.9.3 + Moq 4.20.72 + coverlet 10.0.0

---

## 3. Architecture (Clean Architecture)

The solution follows **Clean Architecture** with 4 layers + presentation apps:

```
┌─────────────────────────────────────────────┐
│            Presentation (WPF Apps)          │
│  SchoolManagement / AttendanceScanner /     │
│  CandidateManagement / Shared Library       │
├─────────────────────────────────────────────┤
│           Application Layer                 │
│  Services, Authorization Handlers,          │
│  Report Generators                          │
├─────────────────────────────────────────────┤
│         Infrastructure Layer                │
│  EF Core DbContext, Repositories,           │
│  PostgreSQL, S3, PDF/Excel Exporters        │
├─────────────────────────────────────────────┤
│             Core Layer                      │
│  Domain Models, Enums, Interfaces,          │
│  Shared Utilities, Attributes               │
└─────────────────────────────────────────────┘
```

### Dependency Rule

Dependencies point **inward**: Presentation → Application → Infrastructure → Core.
- **Core** has zero dependencies on any other project.
- **Infrastructure** depends on Core (and optionally Assets).
- **Application** depends on Core + Infrastructure.
- **Presentation** depends on all lower layers.

---

## 4. Project Structure

```
SchoolManagementSystem/
├── Core/
│   └── SchoolManagement.Core/          # Domain models, enums, interfaces
│       ├── Features/                    # Per-feature domain models
│       │   ├── Students/Models/         # Student, StudentPhoto, etc.
│       │   ├── Attendances/Models/      # Attendance, AttendanceEnum
│       │   ├── Classes/Models/          # Class, ClassSubject
│       │   ├── Reports/Models/          # ReportResult, ReportCardGroup
│       │   └── ...                      # 15+ feature domains
│       └── Shared/                      # Base entity, enums, helpers
│           ├── Contracts/IEntity.cs
│           ├── Enums/
│           └── Extensions/
│
├── Application/
│   └── SchoolManagement.Application/   # Business logic, services
│       ├── Features/
│       │   ├── Reports/
│       │   │   ├── Contracts/           # IReportGenerator, IReportRegistry
│       │   │   ├── Generators/          # StudentRosterGenerator, etc.
│       │   │   ├── Models/              # ReportDefinition, filter models
│       │   │   └── ReportRegistry.cs
│       │   ├── Students/
│       │   │   ├── Contracts/           # IStudentService
│       │   │   ├── Services/            # StudentService
│       │   │   └── Authorization/       # StudentAuthorizationHandler
│       │   └── ...                      # 15+ feature service layers
│       ├── DependencyInjection.cs       # Service registration
│       └── Policies/Docs/               # Authorization documentation
│
├── Infrastructure/
│   └── SchoolManagement.Infrastructure/ # Data access, external services
│       ├── Data/
│       │   ├── SchoolDbContext.cs        # EF Core DbContext (28 DbSets)
│       │   └── Migrations/              # 31 EF Core migrations
│       ├── Features/
│       │   ├── Reports/
│       │   │   ├── Contracts/           # IReportExporter, ICardRenderer
│       │   │   └── Export/
│       │   │       ├── PdfExporter.cs
│       │   │       ├── ExcelExporter.cs
│       │   │       ├── QRCodeExporter.cs
│       │   │       └── Rendering/
│       │   │           └── DefaultCardRenderer.cs
│       │   └── Students/
│       │       ├── Contracts/           # IStudentRepository
│       │       └── Repositories/        # StudentRepository
│       ├── DependencyInjection.cs
│       └── Interceptors/
│           └── AuditSaveChangesInterceptor.cs
│
├── Presentation/
│   ├── Shared/
│   │   └── SchoolManagement.Presentation.Shared/
│   │       ├── Contracts/               # IViewModel, IAsyncLoadable,
│   │       │                             # INavigationService, IDispatcherService
│   │       └── Services/                # NavigationService (shared)
│   │
│   ├── SchoolManagement/               # Main WPF App
│   │   ├── App.xaml / App.xaml.cs       # Entry point, DI, DataTemplates
│   │   ├── Shell/
│   │   │   ├── ViewModels/MainViewModel.cs
│   │   │   └── Views/MainWindow.xaml
│   │   ├── Features/
│   │   │   ├── Reports/
│   │   │   │   ├── Contracts/           # IReportComponentFactory, IReportFilterViewModel
│   │   │   │   ├── Models/              # ReportCardItem, CardPreviewGroup
│   │   │   │   ├── ViewModels/ReportViewModel.cs
│   │   │   │   ├── ViewProviders/       # Per-report filter VMs + Views
│   │   │   │   └── Views/ReportView.xaml
│   │   │   ├── Dashboard/
│   │   │   ├── Students/
│   │   │   ├── Classes/
│   │   │   ├── Attendances/
│   │   │   ├── Scores/
│   │   │   ├── Subjects/
│   │   │   ├── Employees/
│   │   │   ├── Departments/
│   │   │   └── AuditLogs/
│   │   └── Services/ReportComponentFactory.cs
│   │
│   ├── AttendanceScanner/              # QR-scanner app
│   └── CandidateManagement/            # Candidate enrollment app
│
├── Assets/                              # Shared assets (images, fonts, audio)
├── Tests/
│   ├── SchoolManagement.Tests/          # xUnit + Moq tests
│   └── AttendanceScanner.UnitTests/
├── DOCUMENTATION.md                     # This file
└── SchoolManagementSystem.sln
```

---

## 5. How the Application Starts

The entry point is `Presentation/SchoolManagement/App.xaml.cs`.

### App.xaml.cs (key flow)

```csharp
public partial class App : System.Windows.Application
{
    public IHost AppHost { get; }

    public App()
    {
        // 1. Build the DI container via Generic Host
        AppHost = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddInfrastructure();      // DbContext, repos, exporters
                services.AddApplication();         // Services, auth, report generators
                services.AddPresentationShared();  // Shared ViewModels, services

                // Register ViewModels (Transient)
                services.AddTransient<MainViewModel>();
                services.AddTransient<ReportViewModel>();
                // ... 25+ ViewModels

                // Register Views (Transient)
                services.AddTransient<MainWindow>();
                services.AddTransient<ReportView>();
                // ... 15+ Views

                // Register report definitions (Singleton)
                services.AddSingleton<IEnumerable<ReportDefinition>>(sp => [...]);
                services.AddSingleton<IReportComponentFactory, ReportComponentFactory>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        // 2. Start the host
        await AppHost.StartAsync();

        // 3. Resolve MainWindow and set DataContext
        MainWindow mainWindow = serviceProvider.GetRequiredService<MainWindow>();
        MainViewModel mainViewModel = serviceProvider.GetRequiredService<MainViewModel>();
        mainWindow.DataContext = mainViewModel;

        // 4. Show login dialog (blocks until closed)
        LoginViewWindow loginWindow = serviceProvider.GetRequiredService<LoginViewWindow>();
        loginWindow.OnDialogClosed += async (result) =>
        {
            if (result == true)
                mainWindow.Show();  // Show main UI
            else
                Shutdown();         // Exit app
        };
        loginWindow.OpenDialog();
    }
}
```

### App.xaml (DataTemplates)

Each ViewModel type is mapped to its View via a `DataTemplate`. This is how `ContentControl`-based navigation works — WPF automatically resolves the correct View when binding a ViewModel to `ContentControl.Content`.

```xml
<DataTemplate DataType="{x:Type reportVm:ReportViewModel}">
    <reportViews:ReportView />
</DataTemplate>
<DataTemplate DataType="{x:Type studentVm:StudentListViewModel}">
    <studentViews:StudentListView />
</DataTemplate>
<!-- One DataTemplate per ViewModel -->
```

---

## 6. How Navigation Works

### Core Interface

```csharp
public interface INavigationService
{
    IViewModel? CurrentViewModel { get; }
    IViewModel? PreviousViewModel { get; }
    event Action<IViewModel?, IViewModel>? OnViewModelChanged;

    Task NavigateAsync<TViewModel>(INavigationParams? @params = null);
    Task NavigateAsync(Type viewModelType, INavigationParams? @params = null);
    Task<bool?> OpenDialog<TViewModel, TView>(Type? vm = null);
    void ClearCache();
}
```

### Implementation: NavigationService

File: `Presentation/Shared/SchoolManagement.Presentation.Shared/Services/NavigationService.cs`

```csharp
public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDispatcherService _dispatcherService;
    private readonly Dictionary<Type, IViewModel> _vmCache = new();

    public async Task NavigateAsync(Type viewModelType, INavigationParams? @params = null)
    {
        // 1. Resolve or cache ViewModel
        if (!_vmCache.TryGetValue(viewModelType, out IViewModel? viewmodel))
        {
            viewmodel = (IViewModel)_serviceProvider.GetRequiredService(viewModelType);
            _vmCache[viewModelType] = viewmodel;
        }

        // 2. If ViewModel implements INavigationAware, pass navigation params
        if (viewmodel is INavigationAware aware && @params != null)
            await aware.OnNavigatedToAsync(@params);

        // 3. If ViewModel implements IAsyncLoadable, call LoadAsync()
        if (viewmodel is IAsyncLoadable loadable)
            await loadable.LoadAsync();

        // 4. Dispatch to UI thread and fire change event
        await _dispatcherService.InvokeAsync(() =>
        {
            PreviousViewModel = CurrentViewModel;
            CurrentViewModel = viewmodel;
            OnViewModelChanged?.Invoke(PreviousViewModel, CurrentViewModel);
        });
    }
}
```

### Navigation Flow

1. User clicks a button in the sidebar (`MainWindow.xaml`).
2. `MainViewModel` relay command fires, calls `_navigationService.NavigateAsync<SomeViewModel>()`.
3. `NavigationService` resolves/caches the ViewModel, calls `LoadAsync()` if applicable, then updates `CurrentViewModel`.
4. `CustomContentControl` in `MainWindow.xaml` is bound to `CurrentViewModel`:
   ```xml
   <components:CustomContentControl CurrentView="{Binding CurrentViewModel}" ... />
   ```
5. WPF DataTemplate resolves the correct View based on ViewModel type.

### The IAsyncLoadable Check

This is a **critical detail**: `NavigationService` calls `LoadAsync()` **only** if the ViewModel implements `IAsyncLoadable`:

```csharp
if (viewmodel is IAsyncLoadable loadable)
    await loadable.LoadAsync();
```

This means **every ViewModel that needs data on first load must implement `IAsyncLoadable`**. Forgetting this was the root cause of the ReportView showing no data — `ReportViewModel` and the filter ViewModels were missing the interface declaration.

### The Three Key Interfaces Every ViewModel Should Know

| Interface | Purpose |
|-----------|---------|
| `IViewModel` | Empty marker interface — every navigable ViewModel must implement this |
| `IAsyncLoadable` | `Task LoadAsync()` — called when navigation activates this ViewModel |
| `INavigationAware` | `Task OnNavigatedToAsync(INavigationParams)` — receives navigation parameters |

---

## 7. How the Report Feature Works

The report system is the most architecturally complex feature. It follows a **generator + exporter + filter** pattern.

### Architecture Diagram

```
┌──────────────────────────────────────────────────────────────────┐
│                        ReportViewModel                           │
│  (orchestrates cards, filters, preview, export)                  │
└────────────────────┬─────────────────────────────────────────────┘
     │                                        │
     ▼                                        ▼
┌─────────────┐                    ┌──────────────────────┐
│ IReportComp- │                    │     IReportExporter    │
│ onentFactory │                    │  PdfExporter          │
│              │                    │  ExcelExporter        │
└──────┬───────┘                    │  QRCodeExporter       │
       │                            └──────────────────────┘
       ├── creates ──────────────────┐
       │                             ▼
       │                    ┌──────────────────┐
       ├── IReportGenerator │ StudentRoster    │
       │   (by key match)   │ AttendanceReport  │
       │                    │ ScoreReport       │
       │                    │ StudentCard       │
       │                    └──────┬───────────┘
       │                           │
       ▼                           ▼
┌──────────────────┐     ┌────────────────────┐
│ IReportFilterVM   │     │ ReportResult       │
│ (per report type) │     │ (Columns + Rows    │
│                    │     │  or CardGroups)    │
└──────────────────┘     └────────────────────┘
```

### Registration (in App.xaml.cs)

Each report type is registered as a `ReportDefinition`:

```csharp
services.AddSingleton<IEnumerable<ReportDefinition>>(sp =>
[
    new()
    {
        Key = "student-roster",
        DisplayName = "Student Roster",
        GeneratorType = typeof(StudentRosterGenerator),
        FilterViewModelType = typeof(StudentRosterFilterViewModel),
        SortOrder = 1,
    },
    new() { Key = "attendance-report", ... },
    new() { Key = "score-report", ... },
    new() { Key = "student-card", ... },
]);
```

Generators are registered as `IReportGenerator` (multiple implementations):

```csharp
services.AddTransient<IReportGenerator, StudentRosterGenerator>();
services.AddTransient<IReportGenerator, AttendanceReportGenerator>();
services.AddTransient<IReportGenerator, ScoreReportGenerator>();
services.AddTransient<IReportGenerator, StudentCardGenerator>();
```

### IReportGenerator Interface

```csharp
public interface IReportGenerator
{
    string ReportTypeKey { get; }              // Matches ReportDefinition.Key
    object CreateDefaultFilter();
    Task<ReportResult> GenerateAsync(object filter, CancellationToken ct = default);
}
```

Each generator:
1. Defines its `ReportTypeKey` (e.g., `"student-roster"`).
2. Accepts a filter object (cast from `object`).
3. Queries the database via repositories.
4. Returns a `ReportResult` with `Columns`, `Rows`, or `CardGroups`.

### IReportComponentFactory

**Purpose**: Decouples `ReportViewModel` from `IServiceProvider`. The ViewModel never calls `serviceProvider.GetRequiredService()` directly.

```csharp
public interface IReportComponentFactory
{
    IReportGenerator CreateGenerator(ReportDefinition definition);
    IReportFilterViewModel CreateFilterViewModel(ReportDefinition definition);
}
```

Implementation (`ReportComponentFactory.cs`):

```csharp
public class ReportComponentFactory : IReportComponentFactory
{
    private readonly IEnumerable<IReportGenerator> _generators;
    private readonly IServiceProvider _serviceProvider;

    public IReportGenerator CreateGenerator(ReportDefinition definition)
    {
        // Match generator by ReportTypeKey (not by type)
        return _generators.First(g => g.ReportTypeKey == definition.Key);
    }

    public IReportFilterViewModel CreateFilterViewModel(ReportDefinition definition)
    {
        // Resolve filter VM by type (it's registered in DI)
        return (IReportFilterViewModel)_serviceProvider.GetRequiredService(
            definition.FilterViewModelType);
    }
}
```

### ReportViewModel Flow

1. **LoadAsync()** — Called by NavigationService. Gets all `ReportDefinition`s from `IReportRegistry`, builds card items.
2. **SelectReportAsync(card)** — User clicks a report card:
   - Factory creates the right `IReportGenerator` (key-matched).
   - Factory creates the right `IReportFilterViewModel` (type-resolved).
   - Filter VM's `LoadAsync()` is called (if `IAsyncLoadable`).
   - `GeneratePreviewAsync()` is triggered.
3. **GeneratePreviewAsync()** — Calls `_currentGenerator.GenerateAsync(filterData)`:
   - If `result.Layout == ReportLayout.Card`: builds card preview groups.
   - Otherwise: builds a `DataTable` for grid preview.
4. **ExportAsync(exporter)** — User clicks an export button (PDF/Excel):
   - Calls `exporter.ExportToFileAsync(result, filePath)`.
   - Prompts to open the file.

### IReportExporter Interface

```csharp
public interface IReportExporter
{
    string FormatName { get; }          // "PDF", "Excel"
    string FileExtension { get; }       // ".pdf", ".xlsx"
    Task<byte[]> ExportAsync(ReportResult data, CancellationToken ct = default);
    Task ExportToFileAsync(ReportResult data, string filePath, CancellationToken ct = default);
}
```

Two exporters exist: `PdfExporter` (using QuestPDF) and `ExcelExporter` (using ClosedXML).

---

## 8. Key Patterns

### MVVM with CommunityToolkit.Mvvm

ViewModels use source generators:

```csharp
public partial class StudentListViewModel : ObservableObject, IViewModel, IAsyncLoadable
{
    [ObservableProperty]
    private ObservableCollection<Student> _students = [];

    [ObservableProperty]
    private bool _isLoading;

    [RelayCommand]
    private async Task LoadStudentsAsync()
    {
        IsLoading = true;
        var result = await _studentService.GetAllAsync();
        Students = new ObservableCollection<Student>(result);
        IsLoading = false;
    }
}
```

- `[ObservableProperty]` generates the public property + `PropertyChanged` notification.
- `[RelayCommand]` generates an `ICommand` from the method.
- All ViewModels extend `ObservableObject`.

### Dependency Injection

- **Container**: `Microsoft.Extensions.DependencyInjection` (via `Microsoft.Extensions.Hosting`).
- **Lifetimes**:
  - `AddDbContext<SchoolDbContext>(..., ServiceLifetime.Scoped)` — scoped per operation.
  - `AddScoped<T>` — used for services and repositories.
  - `AddTransient<T>` — used for ViewModels, Views, report generators, exporters.
  - `AddSingleton<T>` — used for NavigationService, ReportRegistry, IReportComponentFactory, configuration.
- `ValidateScopes` is **not enabled** — scoped services resolved from the root scope behave as singletons without errors.

### Report Generator Resolution (Keyed Matching)

Instead of registering each generator by concrete type (e.g., `services.AddTransient<StudentRosterGenerator>()`), **all generators are registered as `IReportGenerator`**:

```csharp
services.AddTransient<IReportGenerator, StudentRosterGenerator>();
services.AddTransient<IReportGenerator, AttendanceReportGenerator>();
```

The `ReportComponentFactory` resolves them by matching `ReportTypeKey`:

```csharp
return _generators.First(g => g.ReportTypeKey == definition.Key);
```

This avoids needing to register a type-to-key mapping or use a DI container with named/keyed registration support.

### Authorization (Role-Based)

8 authorization handlers cover all modules (100% coverage):

```csharp
services.AddScoped<IAuthorizationHandler, StudentAuthorizationHandler>();
services.AddScoped<IAuthorizationHandler, AttendanceAuthorizationHandler>();
// ... 6 more
```

Roles: `Admin`, `HeadTeacher`, `Teacher`.

### Navigation with ViewModel Caching

`NavigationService` caches ViewModel instances in a `Dictionary<Type, IViewModel>`. Once a ViewModel is first resolved, the **same instance** is reused on subsequent navigations. Call `ClearCache()` to force re-creation.

### FileSyncBackgroundWorker

A background worker that syncs photos (or other data) from S3 to local cache. Started after login in `App.xaml.cs`.

---

## 9. Creating a New Feature

Here's the step-by-step process to add a new feature (e.g., "Libraries" management):

### 1. Domain Models (Core Layer)

```csharp
// Core/Features/Libraries/Models/Library.cs
public class Library : IEntity, IAuditableEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

### 2. Infrastructure — Repository + EF Config

```csharp
// Infrastructure/Features/Libraries/Contracts/ILibraryRepository.cs
public interface ILibraryRepository : IRepository<Library> { }

// Infrastructure/Features/Libraries/Repositories/LibraryRepository.cs
public class LibraryRepository : Repository<Library>, ILibraryRepository
{
    public LibraryRepository(SchoolDbContext context) : base(context) { }
}

// Register in Infrastructure/DependencyInjection.cs
services.AddScoped<ILibraryRepository, LibraryRepository>();
```

If new DbSet is needed, add to `SchoolDbContext.cs` and create a migration.

### 3. Application — Service + Authorization

```csharp
// Application/Features/Libraries/Contracts/ILibraryService.cs
public interface ILibraryService
{
    Task<IReadOnlyList<Library>> GetAllAsync();
    Task<Library?> GetByIdAsync(Guid id);
    Task AddAsync(Library library);
    Task UpdateAsync(Library library);
    Task DeleteAsync(Guid id);
}

// Application/Features/Libraries/Services/LibraryService.cs
public class LibraryService : ILibraryService { /* implementation */ }

// Register in Application/DependencyInjection.cs
services.AddScoped<ILibraryService, LibraryService>();
```

### 4. Presentation — ViewModel + View

```csharp
// Presentation/SchoolManagement/Features/Libraries/ViewModels/LibraryViewModel.cs
public partial class LibraryViewModel : ObservableObject, IViewModel, IAsyncLoadable
{
    [ObservableProperty] private ObservableCollection<Library> _libraries = [];
    [ObservableProperty] private bool _isLoading;

    public async Task LoadAsync() { /* load data */ }

    [RelayCommand] private async Task AddLibraryAsync() { /* ... */ }
}
```

```xml
<!-- Presentation/SchoolManagement/Features/Libraries/Views/LibraryView.xaml -->
<UserControl ...>
    <DataGrid ItemsSource="{Binding Libraries}" />
</UserControl>
```

### 5. Register in App.xaml + App.xaml.cs

```csharp
// App.xaml.cs
services.AddTransient<LibraryViewModel>();
services.AddTransient<LibraryView>();
```

```xml
<!-- App.xaml -->
<DataTemplate DataType="{x:Type librariesVm:LibraryViewModel}">
    <librariesViews:LibraryView />
</DataTemplate>
```

### 6. Add Navigation Button in MainWindow.xaml + MainViewModel.cs

```csharp
// MainViewModel.cs
[RelayCommand] private void ShowLibrary() =>
    _navigationService.NavigateAsync<LibraryViewModel>();
```

```xml
<!-- MainWindow.xaml -->
<Button Command="{Binding ShowLibraryCommand}">គ្រប់គ្រងបណ្ណាល័យ</Button>
```

---

## 10. Database

### Provider

PostgreSQL via `Npgsql.EntityFrameworkCore.PostgreSQL` 10.0.1.

### DbContext

File: `Infrastructure/SchoolManagement.Infrastructure/Data/SchoolDbContext.cs`

Contains **28 DbSets** including:
- `Students`, `StudentClasses`, `StudentPhotos`, `StudentQRs`
- `Candidates`, `Attendances`, `Classes`, `ClassSubjects`
- `Users`, `Roles`, `Permissions`
- `Employees`, `Departments`, `Subjects`
- `Assessments`, `Scores`, `Exams`
- `Grades`, `Generations`, `Skills`
- `Notifications`, `AuditLogs`

And an `AuditSaveChangesInterceptor` that auto-logs entity changes.

### Migrations

31 EF Core migrations exist (from `20260118124710_Initial_Create` to the latest). Run `Update-Database` (PMC) or `dotnet ef database update` (CLI) to apply.

### Connection String

Read from the **environment variable** `DB_CONNECTION` at runtime via `SecretHelper.GetValueFromEnv("DB_CONNECTION")`:

```csharp
options.UseNpgsql(connectionString, npgsqlOptions =>
{
    npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
});
```

A `.env` file in the Infrastructure project output directory can also supply this value.

---

## 11. Configuration

| Setting | Source | Location |
|---------|--------|----------|
| Database connection | `DB_CONNECTION` env var or `.env` file | Read in `Infrastructure/DependencyInjection.cs` via `SecretHelper` |
| S3 credentials | AWS SDK configuration | `Infrastructure/Features/Files/Services/S3Service.cs` |
| QuestPDF license | `QuestPDF.Settings.License = LicenseType.Community` | `Infrastructure/Features/Reports/Export/PdfExporter.cs:21` |

### .env File Format

```
DB_CONNECTION=Host=localhost;Port=5432;Database=school_management;Username=postgres;Password=yourpassword
```

---

## 12. Build and Run

### Prerequisites

- .NET 10 SDK (or compatible)
- PostgreSQL server running with the database created
- Visual Studio 2022+, VS Code, or JetBrains Rider (optional)

### Steps

```bash
# 1. Restore packages
dotnet restore

# 2. Set connection string (or create .env file)
export DB_CONNECTION="Host=localhost;Port=5432;Database=school_management;Username=postgres;Password=yourpassword"

# 3. Apply database migrations
cd Infrastructure/SchoolManagement.Infrastructure
dotnet ef database update

# 4. Run the main app
cd Presentation/SchoolManagement
dotnet run

# Or build the solution
dotnet build SchoolManagementSystem.sln
```

### Launch Profiles

- **SchoolManagement** — the main app (set as default startup project in solution).
- **AttendanceScanner** — QR attendance tracking app.
- **CandidateManagement** — candidate enrollment app.
- **DataMigrator** — console app to migrate from MS Access to PostgreSQL.
- **SchoolManagement.Tests** — xUnit test project.

### Running Tests

```bash
dotnet test Tests/SchoolManagement.Tests
```

---

## Appendix: Key File Reference

| File | Purpose |
|------|---------|
| `App.xaml.cs` | Entry point, DI registration, startup flow |
| `App.xaml` | DataTemplate mappings (ViewModel → View) |
| `NavigationService.cs` | Core navigation logic with VM caching |
| `IViewModel.cs` | Marker interface for all navigable VMs |
| `IAsyncLoadable.cs` | Interface for VMs that load data on navigation |
| `INavigationAware.cs` | Interface for VMs that receive navigation params |
| `ReportViewModel.cs` | Report hub VM — orchestrates everything |
| `ReportComponentFactory.cs` | Factory that creates generators and filter VMs |
| `IReportGenerator.cs` | Contract for generating report data |
| `IReportFilterViewModel.cs` | Contract for per-report filter controls |
| `IReportExporter.cs` | Contract for PDF/Excel/QR export |
| `PdfExporter.cs` | QuestPDF-based PDF generation |
| `ReportDefinition.cs` | Metadata for each report type |
| `MainViewModel.cs` | Shell VM — sidebar commands, role-based visibility |
| `MainWindow.xaml` | Shell view — sidebar + content area |
| `SchoolDbContext.cs` | EF Core DbContext (28 tables) |
| `DependencyInjection.cs` (Application) | Service and generator registrations |
| `DependencyInjection.cs` (Infrastructure) | DbContext, repo, exporter registrations |
