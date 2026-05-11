using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace SchoolManagement.Presentation.Shared.Components
{
    /// <summary>
    /// Interaction logic for DraggableCorners.xaml
    /// </summary>
    public partial class DraggableBorder : UserControl
    {
        public DraggableBorder()
        {
            InitializeComponent();
        }

        private void LeftThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Window window = Window.GetWindow(this);
            if (window != null)
            {
                double newWidth = window.Width - e.HorizontalChange;
                if (newWidth > window.MinWidth)
                {
                    window.Left += e.HorizontalChange;
                    window.Width = newWidth;
                }
            }
        }

        private void RightThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Window window = Window.GetWindow(this);
            if (window != null)
            {
                double newWidth = window.Width + e.HorizontalChange;
                if (newWidth > window.MinWidth)
                {
                    window.Width = newWidth;
                }
            }
        }

        private void TopThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Window window = Window.GetWindow(this);
            if (window != null)
            {
                double newHeight = window.Height - e.VerticalChange;
                if (newHeight > window.MinHeight)
                {
                    window.Top += e.VerticalChange;
                    window.Height = newHeight;
                }
            }
        }

        private void BottomThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Window window = Window.GetWindow(this);
            if (window != null)
            {
                double newHeight = window.Height + e.VerticalChange;
                if (newHeight > window.MinHeight)
                {
                    window.Height = newHeight;
                }
            }
        }
        private void TopLeftThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            TopThumb_DragDelta(sender, e);
            LeftThumb_DragDelta(sender, e);
        }

        private void TopRightThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            TopThumb_DragDelta(sender, e);
            RightThumb_DragDelta(sender, e);
        }

        private void BottomLeftThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            BottomThumb_DragDelta(sender, e);
            LeftThumb_DragDelta(sender, e);
        }

        private void BottomRightThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            BottomThumb_DragDelta(sender, e);
            RightThumb_DragDelta(sender, e);
        }
    }
}
