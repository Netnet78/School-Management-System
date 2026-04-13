using School_Management.Core.Interfaces.Presentation;
using School_Management.Core.Models;
using System.IO;
using System.Media;

namespace School_Management.Presentation.Shared.Services
{
    public class SoundService : ISoundService
    {
        private List<SoundPlayer> _loadedSoundPlayer = [];

        public void Load(FileObject sound)
        {
            string fullPath = sound.FilePath;
            SoundPlayer player = new(fullPath);

            if (_loadedSoundPlayer.Contains(player)) return;

            player.LoadAsync();
            _loadedSoundPlayer.Add(player);
        }

        public void Load(params FileObject[] sounds)
        {
            foreach (FileObject sound in sounds)
            {
                string fullPath = sound.FilePath;
                SoundPlayer player = new(fullPath);

                player.LoadAsync();
                _loadedSoundPlayer.Add(player);
            }
        }

        public void Play(FileObject sound)
        {
            string fullPath = sound.FilePath;
            SoundPlayer? existingSound = _loadedSoundPlayer.FirstOrDefault(l => l.SoundLocation == fullPath);

            if (existingSound == null)
            {
                existingSound = new(fullPath);
                _loadedSoundPlayer.Add(existingSound);
            }

            existingSound.PlaySync();
        }

        public void Stop(FileObject sound)
        {
            string fullPath = sound.FilePath;
            SoundPlayer? existingSound = _loadedSoundPlayer.FirstOrDefault(l => l.SoundLocation == fullPath);

            existingSound?.Stop();
        }
    }
}
