using School_Management.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace School_Management.Core.Interfaces
{
    public interface IPhotoFetchService
    {
        Task<FileObject?> GetStudentPhoto(string photoKey);
        Task<FileObject?> GetEmployeePhoto(string photoKey);
    }
}
