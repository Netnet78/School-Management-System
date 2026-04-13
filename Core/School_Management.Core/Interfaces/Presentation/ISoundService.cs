using School_Management.Core.Models;

namespace School_Management.Core.Interfaces.Presentation
{
    public interface ISoundService
    {
        public void Load(FileObject sound);
        public void Load(params FileObject[] soundPaths);
        public void Play(FileObject soundPath);
        public void Stop(FileObject soundPath);
    }
}
