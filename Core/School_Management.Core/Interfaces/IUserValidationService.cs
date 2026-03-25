using School_Management.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace School_Management.Core.Interfaces
{
    public interface IUserValidationService
    {
        public Task<User> ValidateUserAsync(string username, string password);
    }
}
