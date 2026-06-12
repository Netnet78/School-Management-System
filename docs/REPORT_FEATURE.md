# Report Feature

## Overview

The report feature generates, previews, and exports structured reports (tabular data or student cards). It follows **Clean Architecture** across four layers and uses a **provider pattern** in the Presentation layer to decouple report type logic from the UI.

There are currently **4 report types**:

| Report | Type | Key | Filters |
|---|---|---|---|
| Student Roster | Table | `student-roster` | Class, Grade, Skill, Include Inactive |
| Attendance Report | Table | `attendance-report` | Class, Date Range |
| Score Report | Table | `score-report` | Class, Subject, Exam |
| Student Name Cards | Card | `student-card` | Class |

---

## Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│  Presentation (WPF)                                             │
│  SchoolManagement.Presentation.Features.Reports                 │
│                                                                 │
│  ReportViewModel ──► IReportViewProvider ──► PreviewView        │
│       │                  │                          │           │
│       │            IReportFilterViewModel       DataTable       │
│       │                                          or            │
│       │                                      CardPreviewGroups  │
│  IReportRegistry ── provides ReportDefinitions                 │
└──────────────────────┬──────────────────────────────────────────┘
                       │ calls
┌──────────────────────▼──────────────────────────────────────────┐
│  Application                                                    │
│  SchoolManagement.Application.Features.Reports                  │
│                                                                 │
│  IReportGenerator ──► GenerateAsync(filter) ──► ReportResult    │
│                                                                 │
│  ReportRegistry  ◄── IReportRegistry                            │
└──────────────────────┬──────────────────────────────────────────┘
                       │ queries
┌──────────────────────▼──────────────────────────────────────────┐
│  Infrastructure                                                │
│  SchoolManagement.Infrastructure.Features.Reports               │
│                                                                 │
│  IReportExporter ──► ExportToFileAsync(result, path)            │
│       │                                                        │
│       ├── ExcelExporter  ──► IExcelRenderer                    │
│       └── PdfExporter    ──► IPdfRenderer                      │
│                                                                 │
│  QRCodeExporter (static utility)                                │
└──────────────────────┬──────────────────────────────────────────┘
                       │ defines
┌──────────────────────▼──────────────────────────────────────────┐
│  Core (Domain)                                                  │
│  SchoolManagement.Core.Features.Reports                         │
│                                                                 │
│  ReportResult (abstract)                                        │
│   ├── TableReportResult  ── Columns + Rows                     │
│   └── CardReportResult   ── CardGroups + CardItems             │
│                                                                 │
│  ReportColumn, CardItem, ReportItemGroup                        │
└─────────────────────────────────────────────────────────────────┘
```

---

## Layer Breakdown

### 1. Core Layer — `SchoolManagement.Core/Features/Reports/`

Domain models with zero dependencies.

| File | Purpose |
|---|---|
| `Models/ReportResult.cs` | Abstract base record with `Title`, `SubTitle`, `GeneratedDate`, `Summary` dictionary |
| `Models/ReportResult.cs` | `TableReportResult` — `List<ReportColumn> Columns` + `List<Dictionary<string, object?>> Rows` |
| `Models/ReportResult.cs` | `CardReportResult` — `List<ReportItemGroup>? CardGroups` |
| `Models/ReportColumn.cs` | Column definition: `Key`, `Header`, `HeaderKhmer`, `DataType`, `Format`, `Width` |
| `Models/CardItem.cs` | Positioned item on a card: `XPos`, `YPos`, `Value` (string or byte[]), `FontSize`, `IsBold`, `FontColor` |
| `Models/ReportItemGroup.cs` | A single card surface: `Width`, `Height`, `List<CardItem> Items` |
| `Contracts/IReportViewModel.cs` | Empty marker interface (unused) |

### 2. Application Layer — `SchoolManagement.Application/Features/Reports/`

Use cases, interfaces, and generators.

**Contracts:**

| Interface | Responsibility |
|---|---|
| `IReportGenerator` | `ReportTypeKey`, `CreateDefaultFilter()`, `GenerateAsync(filter)` → `ReportResult` |
| `IReportRegistry` | `GetAll()` → `IReadOnlyList<ReportDefinition>`, `GetByKey(key)` |

**Models:**

| Model | Properties |
|---|---|
| `ReportDefinition` | `Key`, `DisplayName`, `DisplayNameKhmer`, `Description`, `IconKind`, `SortOrder` |
| `AttendanceReportFilter` | `ClassId?`, `DateFrom?`, `DateTo?` |
| `ScoreReportFilter` | `ClassId?`, `SubjectId?`, `ExamId?` |
| `StudentRosterFilter` | `ClassId?`, `GradeId?`, `SkillId?`, `IsActive?` |
| `StudentCardFilter` | `ClassId?` |

**Generators** each query repositories and assemble a `ReportResult`:

| Generator | Output Type | Data Source |
|---|---|---|
| `StudentRosterGenerator` | Table | `IStudentClassRepository` (with `Student`, `Candidate`, `Class`, `Grade`, `Skill` includes) |
| `AttendanceReportGenerator` | Table | `IAttendanceRepository` (with `StudentClass.Student.Candidate`, `Class` includes) |
| `ScoreReportGenerator` | Table | `IAccessmentRepository` (with `StudentClass.Student.Candidate`, `ClassSubject.Subject`, `Exam`, `Scores.Component` includes) |
| `StudentCardGenerator` | Card | `IStudentClassRepository` + `IPhotoFetchService` + `QRCodeExporter` |

**Registry:** `ReportRegistry` wraps `IEnumerable<ReportDefinition>` in a `Dictionary<string, ReportDefinition>` keyed by `Key`, ordered by `SortOrder`.

### 3. Infrastructure Layer — `SchoolManagement.Infrastructure/Features/Reports/`

File export and rendering.

**Contracts:**

| Interface | Methods |
|---|---|
| `IReportExporter` | `FormatName`, `FileExtension`, `TemplatePath`, `ExportAsync(data)`, `ExportToFileAsync(data, filePath)` |
| `IExcelRenderer` | `Render(TableReportResult, XLWorkbook?)` → `XLWorkbook` |
| `IPdfRenderer` | `Render(IContainer, ReportItemGroup)` |

**Exporters:**

| Class | Format | Notes |
|---|---|---|
| `ExcelExporter` | `.xlsx` | Uses ClosedXML + `IExcelRenderer`. Throws if given `CardReportResult`. |
| `PdfExporter` | `.pdf` | Uses QuestPDF. Handles both `TableReportResult` (table page) and `CardReportResult` (card pages, 8 cards per A4 page). |

**Renderers:**

| Class | Implements | Purpose |
|---|---|---|
| `DefaultExcelTemplateRenderer` | `IExcelRenderer` | Writes title, headers, data rows, and summary into an Excel worksheet |
| `DefaultPdfTemplateRenderer` | `IPdfRenderer` | Stub — throws `NotImplementedException` |
| `StudentCardRenderer` | `IPdfRenderer` | Renders a single card from a template image with overlaid text/images |

**QRCodeExporter:** Static utility class that generates QR code PNG bytes using ZXing + SkiaSharp. Used by `StudentCardGenerator` to embed QR codes on name cards.

**DI Registration** (`Infrastructure/DependencyInjection.cs`):
```csharp
services.AddTransient<IReportExporter, ExcelExporter>();
services.AddTransient<IReportExporter, PdfExporter>();
services.AddTransient<IExcelRenderer, DefaultExcelTemplateRenderer>();
services.AddTransient<IPdfRenderer, StudentCardRenderer>();
```

### 4. Presentation Layer — `SchoolManagement.Presentation/Features/Reports/`

WPF UI layer — ViewModels, Views, Providers, and Filters.

**Contracts:**

| Interface | Members |
|---|---|
| `IReportViewProvider` | `ReportTypeKey`, `FilterViewModel`, `PreviewView`, `HasData`, `SummaryText`, `GenerateAsync(filter)`, `ExportAsync(exporter, path)` |
| `IReportFilterViewModel` | `ReportTypeKey`, event `FilterChanged`, `GetFilterData()`, `ResetFilterData()` |
| `IReportComponentFactory` | `CreateGenerator(definition)`, `CreateFilterViewModel(definition)` — unused, superseded by provider pattern |

**Models:**

| Model | Purpose |
|---|---|
| `ReportCardItem` (`ObservableObject`) | Wraps `ReportDefinition` for UI display + selection state |
| `CardPreviewItem` | `X`, `Y`, `Text`, `ImageBytes`, `ImageSource` — preview representation of a `CardItem` |
| `CardPreviewGroup` | `Width`, `Height`, `ObservableCollection<CardPreviewItem> Items` |

**Providers (abstract bases):**

| Class | Purpose | Creates |
|---|---|---|
| `TableReportViewProvider` | Converts `TableReportResult` → `DataTable` for DataGrid binding | `ReportTablePreviewView` |
| `CardReportViewProvider` | Converts `CardReportResult` → `ObservableCollection<CardPreviewGroup>` for card display | `ReportCardPreviewView` |

Both bases share the same pattern:
- Store `_lastFilter` + `_lastResult`
- `GenerateAsync` calls the abstract `GenerateReportAsync`, processes the result
- `ExportAsync` re-generates data (not cached) then calls `exporter.ExportToFileAsync`

**Concrete Providers** (all follow identical pattern — inject `IEnumerable<IReportGenerator>` + `IEnumerable<IReportFilterViewModel>`, resolve by key):

| Class | Key | Base Class | Filter VM |
|---|---|---|---|
| `StudentRosterProvider` | `student-roster` | `TableReportViewProvider` | `StudentRosterFilterViewModel` |
| `AttendanceReportProvider` | `attendance-report` | `TableReportViewProvider` | `AttendanceFilterViewModel` |
| `ScoreReportProvider` | `score-report` | `TableReportViewProvider` | `ScoreFilterViewModel` |
| `StudentCardProvider` | `student-card` | `CardReportViewProvider` | `StudentCardFilterViewModel` |

**Filter ViewModels:**

| VM | Properties | FilterChanged triggers on |
|---|---|---|
| `StudentRosterFilterViewModel` | `Classes`, `SelectedClass`, `Grades`, `SelectedGrade`, `Skills`, `SelectedSkill`, `IncludeInactive` | All properties |
| `AttendanceFilterViewModel` | `Classes`, `SelectedClass`, `DateFrom`, `DateTo` | All properties |
| `ScoreFilterViewModel` | `Classes`, `SelectedClass`, `Subjects`, `SelectedSubject`, `Exams`, `SelectedExam` | All properties; changing class loads subjects async |
| `StudentCardFilterViewModel` | `Classes`, `SelectedClass` | `SelectedClass` |

All filter VMs implement `IReportFilterViewModel` + `IAsyncLoadable`. They are resolved by the Presentation layer's `ContentControl` via DataTemplates in `App.xaml`.

**ReportViewModel** (main orchestrator):

| Aspect | Detail |
|---|---|
| Injects | `IReportRegistry`, `IEnumerable<IReportViewProvider>`, `IMessageService`, `IEnumerable<IReportExporter>` |
| Commands | `SelectReportCommand`, `GenerateReportCommand` (via filter), `ExportCommand`, `ClearFiltersCommand` |
| Key logic | `LoadAsync` → loads definitions, auto-selects first card. `SelectReportAsync` → finds provider, wires filter, triggers initial generate. `OnFilterChanged` → regenerates on any filter change. `ExportAsync` → saves file dialog, delegates to provider, asks to open. |
| Exporters filter | `SelectedCard?.Key == "student-card"` → only PDF exporters shown (cards can't export to Excel) |

**Views:**

| View | Type | Binding |
|---|---|---|
| `ReportView` | Main container | DataContext = `ReportViewModel` |
| `ReportTablePreviewView` | `DataGrid` | `ItemsSource = {Binding DataTable}` (auto-generated columns) |
| `ReportCardPreviewView` | `ItemsControl` of card borders with `Canvas` layout | `ItemsSource = {Binding CardPreviewGroups}` |

**DI Registration** (`App.xaml.cs`):
```csharp
// Report definitions (singleton)
services.AddSingleton<IEnumerable<ReportDefinition>>(sp => [... 4 definitions]);

// Report registry
services.AddSingleton<IReportRegistry, ReportRegistry>();

// Providers
services.AddTransient<IReportViewProvider, StudentRosterProvider>();
services.AddTransient<IReportViewProvider, AttendanceReportProvider>();
services.AddTransient<IReportViewProvider, ScoreReportProvider>();
services.AddTransient<IReportViewProvider, StudentCardProvider>();

// Filter VMs
services.AddTransient<IReportFilterViewModel, StudentRosterFilterViewModel>();
services.AddTransient<IReportFilterViewModel, AttendanceFilterViewModel>();
services.AddTransient<IReportFilterViewModel, ScoreFilterViewModel>();
services.AddTransient<IReportFilterViewModel, StudentCardFilterViewModel>();

// Views (transient)
services.AddTransient<ReportView>();
services.AddTransient<ReportTablePreviewView>();
services.AddTransient<ReportCardPreviewView>();

// Filter Views (for DataTemplate resolution)
services.AddTransient<StudentRosterFilterView>();
services.AddTransient<AttendanceFilterView>();
services.AddTransient<ScoreFilterView>();
services.AddTransient<StudentCardFilterView>();
```

---

## Complete Data Flow

```
1. User navigates to Reports
   └── Shell navigates to ReportView (DataTemplate maps ReportViewModel → ReportView)

2. ReportViewModel.LoadAsync()
   ├── Loads all ReportDefinitions from IReportRegistry
   ├── Creates ReportCardItem wrappers
   ├── Auto-selects first card
   └── Calls SelectReportAsync(cards[0])

3. SelectReportAsync(card)
   ├── Finds IReportViewProvider with matching ReportTypeKey
   ├── Unsubscribes previous filter's FilterChanged event
   ├── If filter VM exists:
   │   ├── Subscribes to FilterChanged
   │   ├── Sets CurrentFilterViewModel (rendered via DataTemplate)
   │   └── Calls IAsyncLoadable.LoadAsync() on filter VM
   ├── Sets CurrentPreviewView (provider-owned UserControl)
   └── Calls GenerateWithProviderAsync(initial filter)

4. GenerateWithProviderAsync(filter)
   ├── Sets IsLoading = true
   ├── Calls provider.GenerateAsync(filter)
   │   └── provider calls generator.GenerateAsync(filter)
   │       ├── Builds ADO.NET/EF query with filter conditions
   │       ├── Executes via repository
   │       └── Returns ReportResult (TableReportResult or CardReportResult)
   ├── Provider converts result:
   │   ├── TableReportResult → DataTable (columns + rows)
   │   └── CardReportResult → CardPreviewGroups (positioned items)
   ├── Sets HasData, SummaryText
   └── Sets IsLoading = false

5. User changes a filter value
   ├── Filter VM raises FilterChanged event
   ├── ReportViewModel.OnFilterChanged → GenerateWithProviderAsync(new filter)
   └── Preview view updates automatically (bound to provider's properties)

6. User clicks Export button (for a specific exporter)
   ├── ReportViewModel.ExportAsync(exporter)
   ├── Shows SaveFileDialog with exporter's extension/filter
   ├── Calls provider.ExportAsync(exporter, filePath)
   │   └── provider re-generates data, calls exporter.ExportToFileAsync(result, filePath)
   ├── Shows success dialog
   └── If user clicks Yes, opens file via Process.Start with UseShellExecute
```

---

## Adding a New Report Type

### Step 1: Core Layer (if new result type needed)
- Add a new record in `ReportResult.cs` inheriting from `ReportResult` if neither `TableReportResult` nor `CardReportResult` fits

### Step 2: Application Layer
- **Filter model**: Add a new filter class in `Models/` (e.g., `CustomReportFilter.cs`)
- **Generator**: Implement `IReportGenerator` in `Generators/` with:
  - `ReportTypeKey` (unique string)
  - `CreateDefaultFilter()` returning default filter instance
  - `GenerateAsync(filter)` returning `TableReportResult` or `CardReportResult`
- **Register** in `Application/DependencyInjection.cs`:
  ```csharp
  services.AddTransient<IReportGenerator, CustomReportGenerator>();
  ```

### Step 3: Infrastructure Layer (if new export format needed)
- Implement `IReportExporter` in `Export/`
- Implement `IExcelRenderer` or `IPdfRenderer` if custom rendering logic is needed

### Step 4: Presentation Layer

- **Report definition** — add entry in `App.xaml.cs`:
  ```csharp
  new() {
      Key = "custom-report",
      DisplayName = "Custom Report",
      DisplayNameKhmer = "របាយការណ៍ផ្ទាល់ខ្លួន",
      Description = "...",
      IconKind = "ChartBar",
      SortOrder = 5,
  }
  ```

- **Filter View + VM** — create `ViewProviders/Custom/` folder with:
  - `CustomFilterView.xaml` + `.xaml.cs`
  - `CustomFilterViewModel.cs` implementing `IReportFilterViewModel, IAsyncLoadable`
  - Register VM: `services.AddTransient<IReportFilterViewModel, CustomFilterViewModel>()`
  - Add DataTemplate in `App.xaml` mapping VM to View
  - Register view: `services.AddTransient<CustomFilterView>()`

- **Provider** — create `Providers/CustomReportProvider.cs`:
  ```csharp
  public class CustomReportProvider : TableReportViewProvider  // or CardReportViewProvider
  {
      // Same pattern as existing providers (see StudentRosterProvider)
      private readonly IReportGenerator _generator;
      private readonly IReportFilterViewModel _filterVm;

      public override string ReportTypeKey => "custom-report";
      public override IReportFilterViewModel? FilterViewModel => _filterVm;

      public CustomReportProvider(
          IEnumerable<IReportGenerator> generators,
          IEnumerable<IReportFilterViewModel> filterVms)
      {
          _generator = generators.First(g => g.ReportTypeKey == "custom-report");
          _filterVm = filterVms.First(f => f.ReportTypeKey == "custom-report");
      }

      protected override async Task<ReportResult> GenerateReportAsync(object filter, CancellationToken ct)
          => await _generator.GenerateAsync(filter, ct);
  }
  ```

- **Register provider**: `services.AddTransient<IReportViewProvider, CustomReportProvider>()`

---

## Design Notes & Quirks

- **Provider ↔ Generator relationship**: Each provider resolves its generator by key from `IEnumerable<IReportGenerator>` using `First()`. If the key doesn't match, it throws. This means every generator must have a matching provider key.

- **Data re-generation on export**: Both `TableReportViewProvider.ExportAsync` and `CardReportViewProvider.ExportAsync` call `GenerateReportAsync` again instead of using the cached `_lastResult`. This ensures fresh data on export but is less efficient.

- **`DefaultPdfTemplateRenderer`** is a stub — `PdfExporter` handles PDF generation directly via inline QuestPDF code in `PdfExporter.CreateDocument()`. The renderer interface is only used for the student card rendering (via `StudentCardRenderer`).

- **`IReportComponentFactory`** is registered but unused. The provider pattern (injecting `IEnumerable<T>` + resolving by key) replaced the factory approach.

- **`IReportViewModel`** in Core is an empty marker interface with no consumers.

- The `ScoreView.xaml` (in Features/Scores) contains a standalone "Generate Report" button that builds a plain-text summary in `ScoreViewModel.GenerateReportAsync()` and displays it in a message box. This is separate from the report feature described above.

- **Exporters are filtered** in `ReportViewModel.Exporters`: when the selected card is `"student-card"`, only PDF exporters are shown (Excel cannot render cards).
