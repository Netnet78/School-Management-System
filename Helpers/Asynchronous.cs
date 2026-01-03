using System;
using System.Collections.Generic;
using System.Text;

namespace New_Student_Management.Helpers
{
    public interface IAsyncLoadable
    {
        Task LoadAsync();
    }
}
