using SchoolManagement.Core.Shared.Models;

namespace SchoolManagement.Core.Shared.Presentation.Contracts
{

    public interface IFrameProcessingService
    {
        /// <summary>
        /// Processes a single camera frame by copying its raw pixel data into an internal buffer,
        /// converting it into a <c>WriteableBitmap</c> for rendering in the UI, and ensuring
        /// that unmanaged resources associated with the frame are properly released.
        ///
        /// This method acts as the bridge between low-level camera frame data and WPF-compatible
        /// image rendering. It updates or initializes the internal bitmap as needed and writes
        /// the frame's pixel data efficiently using <c>WriteableBitmap</c>.WritePixels() function.
        ///
        /// The input <see cref="CameraFrame"/> is always disposed after processing to prevent
        /// memory leaks and ensure proper resource management.
        ///
        /// </summary>
        /// <param name="frame">
        /// The camera frame containing raw pixel data, dimensions, and stride information.
        /// </param>
        /// <returns>
        /// A <c>WriteableBitmap</c> instance object representing the rendered frame,
        /// ready for display in the UI.
        /// </returns>
        object ProcessFrame(CameraFrame frame);
    }
}
