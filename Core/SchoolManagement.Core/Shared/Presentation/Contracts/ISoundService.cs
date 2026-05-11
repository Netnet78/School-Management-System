using SchoolManagement.Core.Shared.Models;

namespace SchoolManagement.Core.Shared.Presentation.Contracts
{
    public interface ISoundService
    {
        public void Load(SoundObject sound);
        public void Load(params SoundObject[] soundPaths);
        public void Play(SoundObject soundPath);
        public void Stop(SoundObject soundPath);
    }
}
