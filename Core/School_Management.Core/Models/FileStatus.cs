using School_Management.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace School_Management.Core.Models
{
    public class FileStatus : ReturnResponse<FileObject>
    {
        public FileLocationOptions UploadedLocation { get; set; } = FileLocationOptions.LocalAndOnline;

    }
}
