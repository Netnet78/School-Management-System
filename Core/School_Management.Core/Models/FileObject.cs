namespace School_Management.Core.Models;

/// <summary>
/// A class that contains information about the file and is initialized by a path
/// </summary>
public class FileObject
{
    public string FilePath { get; private set; }
    public string FullFileName { get; private set; }
    public string FileName { get; private set; }
    public string FileExtension { get; private set; }

    public FileObject(string path)
    {
        FilePath = path;
        FullFileName = Path.GetFileName(path);
        FileName = Path.GetFileNameWithoutExtension(path);
        FileExtension = Path.GetExtension(path);
    }
}
