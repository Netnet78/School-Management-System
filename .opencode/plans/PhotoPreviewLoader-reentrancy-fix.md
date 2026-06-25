# Plan: Fix PhotoPreviewLoader reentrancy + add async API

## 1. Fix reentrancy bug + add LoadAsync

### File: `Presentation/Shared/SchoolManagement.Presentation.Shared/XAMLConverters/PhotoPreviewLoader.cs`

**A) Strip caching from `LoadFromUri`** (line 41-51):

Remove the `Cache.GetOrAdd` wrapper so it just creates and returns the bitmap:

```csharp
private static ImageSource? LoadFromUri(Uri uri, int decodePixelWidth, int decodePixelHeight)
{
    return CreateBitmapImage(bitmap =>
    {
        bitmap.UriSource = uri;
        ApplyDecodeHints(bitmap, decodePixelWidth, decodePixelHeight);
    });
}
```

**B) Strip caching from `LoadFromFile`** (line 53-64):

Same treatment — remove `Cache.GetOrAdd`:

```csharp
private static ImageSource? LoadFromFile(string fullPath, int decodePixelWidth, int decodePixelHeight)
{
    return CreateBitmapImage(bitmap =>
    {
        using FileStream stream = new(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        bitmap.StreamSource = stream;
        ApplyDecodeHints(bitmap, decodePixelWidth, decodePixelHeight);
    });
}
```

**C) Move URI caching into `Load`** (line 17-38):

Update `Load` to handle caching for both file and URI paths via `Lazy`:

```csharp
public static ImageSource? Load(string? path, int decodePixelWidth = 240, int decodePixelHeight = 0)
{
    if (string.IsNullOrWhiteSpace(path))
    {
        return null;
    }

    if (Uri.TryCreate(path, UriKind.Absolute, out Uri? uri) && uri.Scheme != Uri.UriSchemeFile)
    {
        string cacheKey = $"uri|{uri}|{decodePixelWidth}|{decodePixelHeight}";
        return Cache.GetOrAdd(cacheKey, _ => new Lazy<ImageSource?>(() =>
            LoadFromUri(uri, decodePixelWidth, decodePixelHeight),
            LazyThreadSafetyMode.ExecutionAndPublication)).Value;
    }

    string fullPath = Path.GetFullPath(path);
    if (!File.Exists(fullPath))
    {
        return null;
    }

    string cacheKey = $"file|{fullPath}|{decodePixelWidth}|{decodePixelHeight}";
    return Cache.GetOrAdd(cacheKey, _ => new Lazy<ImageSource?>(() =>
        LoadFromFile(fullPath, decodePixelWidth, decodePixelHeight),
        LazyThreadSafetyMode.ExecutionAndPublication)).Value;
}
```

**D) Add `LoadAsync` method**:

```csharp
public static Task<ImageSource?> LoadAsync(string? path, int decodePixelWidth = 240, int decodePixelHeight = 0, CancellationToken cancellationToken = default)
{
    if (string.IsNullOrWhiteSpace(path))
    {
        return Task.FromResult<ImageSource?>(null);
    }

    return Task.Run(() => Load(path, decodePixelWidth, decodePixelHeight), cancellationToken);
}
```

Add `using System.Threading;` (already present) and `using System.Threading.Tasks;` at top.

---

## 2. Update ViewModel call sites

### File: `Presentation/SchoolManagement/Features/Students/ViewModels/AssignCandidateViewModel.cs` (line 158-160)

Replace:
```csharp
SelectedCandidatePhotoPreview = await Task.Run(
    () => PhotoPreviewLoader.Load(SelectedCandidatePhoto?.FilePath, 220),
    token);
```
With:
```csharp
SelectedCandidatePhotoPreview = await PhotoPreviewLoader.LoadAsync(
    SelectedCandidatePhoto?.FilePath, 220, token);
```

### File: `Presentation/CandidateManagement/ViewModels/StudentViewModel.cs` (line 147-149)

Replace:
```csharp
SelectedStudentPhotoPreview = await Task.Run(
    () => PhotoPreviewLoader.Load(SelectedStudentPhoto, 220),
    token);
```
With:
```csharp
SelectedStudentPhotoPreview = await PhotoPreviewLoader.LoadAsync(
    SelectedStudentPhoto, 220, token);
```

### File: `Presentation/SchoolManagement/Features/Students/ViewModels/StudentFormViewModelBase.cs` (line 265-267)

Replace:
```csharp
ImageSource? preview = await Task.Run(
    () => PhotoPreviewLoader.Load(photoPath, 220),
    cts.Token).ConfigureAwait(true);
```
With:
```csharp
ImageSource? preview = await PhotoPreviewLoader.LoadAsync(
    photoPath, 220, cts.Token).ConfigureAwait(true);
```
