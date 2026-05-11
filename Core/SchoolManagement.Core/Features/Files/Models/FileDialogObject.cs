namespace SchoolManagement.Core.Models
{
    public class FileDialogObject
    {
        public FileObject[] Files { get; set; }
        public FileObject? File { get; set; }

        public FileDialogObject(FileObject[] files)
        {
            Files = files;
            File = Files[0];
        }

        public FileDialogObject(FileObject? file)
        {
            Files = file == null ?[] : [file];
            File = file;
        }

        public string GetFilePath(int index=0)
        {
            return Files[index].FilePath;
        }

        public string GetFilePath(FileObject file)
        {
            FileObject? existing = Files.FirstOrDefault(f => f.FileKey == file.FileKey);
            return existing?.FilePath ?? string.Empty;
        }

        public string GetFilePath(string fullFileName)
        {
            FileObject? existing = Files.FirstOrDefault(f => fullFileName == f.FileKey);
            return existing?.FilePath ?? string.Empty;
        }
    }
}
