using System.IO;

namespace SchoolManagement.Assets
{
    // Static helper class that provides computed paths to each resource subfolder on disk.
    public static class ResourcePaths
    {
        // BaseDirectory: the folder where the application's executable lives at runtime.
        // readonly static: computed once and never reassigned.
        private readonly static string baseFolder =
            AppDomain.CurrentDomain.BaseDirectory;

        // Expression-bodied property (=>): computed fresh each time it is read.
        // Path.Combine joins the base folder with a subfolder name in an OS-safe way.
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
