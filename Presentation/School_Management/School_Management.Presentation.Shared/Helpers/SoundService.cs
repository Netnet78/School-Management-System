using System.IO;
using System.Media;

namespace School_Management.Presentation.Shared.Helpers
{
    public static class SoundService
    {
        private static SoundPlayer _player = null!;
        public static void Play()
        {
            _player.Play();
        }

        public static void Load(string file)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, file);
            _player = new(path);
        }

        public static void Stop(SoundPlayer player)
        {
            player.Stop();
        }
    }
}
