using System;
using System.Collections.Generic;
using System.Text;

namespace School_Management.Core.Models
{
    public class CameraFrame
    {
        public nint Data { get; set; } = default!;
        public int Width { get; set; }
        public int Height { get; set; }
        public int Stride { get; set; }

        public CameraFrame(CameraFrame data)
        {
            Data = data.Data;
            Width = data.Width;
            Height = data.Height;
            Stride = data.Stride;
        }

        public CameraFrame()
        {

        }
    }
}
