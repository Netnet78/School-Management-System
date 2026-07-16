namespace SchoolManagement.Presentation.Shared.Models
{
    public class SoundObject : FileObject
    {
        public string SoundId { get; private set; }
        public SoundObject(string path) : base(path)
        {
            SoundId = Guid.NewGuid().ToString();
        }
    }
}
