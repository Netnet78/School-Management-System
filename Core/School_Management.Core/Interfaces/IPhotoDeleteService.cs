using School_Management.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace School_Management.Core.Interfaces
{
    public interface IPhotoDeleteService
    {
        Task<FileObject> DeleteStudentPhoto(string photoKey);
        Task<FileObject> DeleteEmployeePhoto(string photoKey);
    }
}
