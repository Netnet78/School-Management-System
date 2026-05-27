using System.Collections.ObjectModel;
using System.Windows.Media;

namespace SchoolManagement.Presentation.Features.Reports.Models
{
    public class CardPreviewItem
    {
        public double X { get; set; }
        public double Y { get; set; }
        public string? Text { get; set; }
        public byte[]? ImageBytes { get; set; }
        public ImageSource? ImageSource { get; set; }
    }

    public class CardPreviewGroup
    {
        public double Width { get; set; }
        public double Height { get; set; }
        public ObservableCollection<CardPreviewItem> Items { get; set; } = [];
    }
}
