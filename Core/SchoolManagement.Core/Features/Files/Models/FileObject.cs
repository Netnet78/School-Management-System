namespace SchoolManagement.Core.Features.Files.Models;

public class FileObject
{
    public string FileKey { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;

    public string FilePath => FullPath;

    public FileObject(string path)
    {
        FullPath = path;
        FileKey = Path.GetFileName(path);
    }
}
