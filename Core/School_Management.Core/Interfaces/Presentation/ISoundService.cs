using School_Management.Core.Models;

namespace School_Management.Core.Interfaces.Presentation
{
    public interface ISoundService
    {
        public void Load(SoundObject sound);
        public void Load(params SoundObject[] soundPaths);
        public void Play(SoundObject soundPath);
        public void Stop(SoundObject soundPath);
    }
}
