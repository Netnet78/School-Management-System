using School_Management.Core.Interfaces.Presentation;
using School_Management.Core.Models;
using System.Media;

namespace School_Management.Presentation.Shared.Services
{

    public class SoundService : ISoundService
    {
        internal class SoundMetaData
        {
            required public SoundPlayer Sound { get; set; }
            required public string SoundKey { get; set; }
        }

        private List<SoundMetaData> _loadedSoundPlayer = [];

        public void Load(SoundObject sound)
        {
            string fullPath = sound.FilePath;
            SoundPlayer player = new(fullPath);

            string[] keys = _loadedSoundPlayer.Select(s => s.SoundKey).ToArray();

            if (keys.Contains(sound.SoundId)) return;

            player.LoadAsync();
            _loadedSoundPlayer.Add(new() { Sound = player, SoundKey = sound.SoundId });
        }

        public void Load(params SoundObject[] sounds)
        {
            foreach (SoundObject sound in sounds)
            {
                string fullPath = sound.FilePath;
                SoundPlayer player = new(fullPath);

                string[] keys = _loadedSoundPlayer.Select(s => s.SoundKey).ToArray();

                if (keys.Contains(sound.SoundId)) return;

                player.LoadAsync();
                _loadedSoundPlayer.Add(new() { Sound = player, SoundKey = sound.SoundId });
            }
        }

        public void Play(SoundObject sound)
        {
            string fullPath = sound.FilePath;
            SoundPlayer? existingSound = _loadedSoundPlayer.FirstOrDefault(l => l.SoundKey == sound.SoundId)?.Sound;

            if (existingSound == null)
            {
                existingSound = new(fullPath);
                _loadedSoundPlayer.Add(new() { Sound = existingSound, SoundKey = sound.SoundId });
            }

            existingSound.PlaySync();
        }

        public void Stop(SoundObject sound)
        {
            string fullPath = sound.FilePath;
            SoundPlayer? existingSound = _loadedSoundPlayer.FirstOrDefault(l => l.SoundKey == fullPath)?.Sound;

            existingSound?.Stop();
        }
    }
}
