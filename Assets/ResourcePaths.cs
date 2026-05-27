namespace SchoolManagement.Assets
{
    public static class ResourcePaths
    {
        private readonly static string baseFolder =
            AppDomain.CurrentDomain.BaseDirectory;
        public static string Fonts =>
            Path.Combine(baseFolder, "Fonts");

        public static string Audio =>
            Path.Combine(baseFolder, "Audio");

        public static string Images =>
            Path.Combine(baseFolder, "Images");

        public static string Spreadsheets =>
            Path.Combine(baseFolder, "Spreadsheets");
    }
}
