namespace SchoolManagement.Assets;

public static class FontResourceResolver
{
    private const string FontPackBaseUri = "pack://application:,,,/SchoolManagement.Assets;component/Fonts/";

    private static readonly IReadOnlyDictionary<string, string> EmbeddedFontSources =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Noto Sans Khmer"] = $"{FontPackBaseUri}#Noto Sans Khmer",
            ["Khmer OS Battambang"] = $"{FontPackBaseUri}#Khmer OS Battambang",
            ["Khmer OS Bokor"] = $"{FontPackBaseUri}#Khmer OS Bokor",
            ["Khmer OS Muol Light"] = $"{FontPackBaseUri}#Khmer OS Muol Light",
            ["Khmer OS Muol"] = $"{FontPackBaseUri}#Khmer OS Muol",
            ["Khmer OS Siemreap"] = $"{FontPackBaseUri}#Khmer OS Siemreap",
        };

    public static string ResolveFontSource(string? fontFamily)
    {
        if (string.IsNullOrWhiteSpace(fontFamily))
            return "Times New Roman";

        string normalized = fontFamily.Trim();

        return EmbeddedFontSources.TryGetValue(normalized, out string? source)
            ? source
            : normalized;
    }
}
