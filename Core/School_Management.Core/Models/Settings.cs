using System;
using System.Collections.Generic;
using System.Text;

namespace School_Management.Core.Models
{
    public class Settings
    {
        public string Theme { get; set; } = string.Empty;
        public string StudentPhotoFolderPath { get; set; } = string.Empty;
        public string EmployeePhotoFolderPath { get; set; } = string.Empty;
        public string StudentPhotoFolderBucketPath { get; set; } = string.Empty;
        public string EmployeePhotoFolderBucketPath { get; set; } = string.Empty;
    }
}
