namespace SchoolManagement.Assets;

// Static helper class: maps friendly font family names to their WPF pack URIs for embedded fonts.
public static class FontResourceResolver
{
    // Base URI prefix for fonts embedded inside the SchoolManagement.Assets assembly resource pack.
    private const string FontPackBaseUri = "pack://application:,,,/SchoolManagement.Assets;component/Fonts/";

    // IReadOnlyDictionary<TKey, TValue>: a read-only key/value map; here it stores font-name → pack URI pairs.
    // StringComparer.OrdinalIgnoreCase makes the lookup case-insensitive (e.g. "noto sans khmer" still matches).
    private static readonly IReadOnlyDictionary<string, string> EmbeddedFontSources =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // Each entry maps the human-readable font name to its WPF pack URI (the # suffix selects the face).
            ["Noto Sans Khmer"] = $"{FontPackBaseUri}#Noto Sans Khmer",
            ["Khmer OS Battambang"] = $"{FontPackBaseUri}#Khmer OS Battambang",
            ["Khmer OS Bokor"] = $"{FontPackBaseUri}#Khmer OS Bokor",
            ["Khmer OS Muol Light"] = $"{FontPackBaseUri}#Khmer OS Muol Light",
            ["Khmer OS Muol"] = $"{FontPackBaseUri}#Khmer OS Muol",
            ["Khmer OS Siemreap"] = $"{FontPackBaseUri}#Khmer OS Siemreap",
        };

    // Resolves a font family name to its embedded pack URI, or falls back to a system font name.
    // fontFamily?: the ? means the parameter is nullable — callers may pass null.
    public static string ResolveFontSource(string? fontFamily)
    {
        // Guard: if the name is null, empty, or only whitespace, default to a safe system font.
        if (string.IsNullOrWhiteSpace(fontFamily))
            return "Times New Roman";

        string normalized = fontFamily.Trim(); // Remove leading/trailing spaces before lookup.

        // TryGetValue: looks up the key; if found, puts the value into `source` and returns true.
        // The ternary ? : returns the pack URI when found, or the trimmed name for system fonts.
        return EmbeddedFontSources.TryGetValue(normalized, out string? source)
            ? source
            : normalized;
    }
}
