using New_Student_Management.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace New_Student_Management.Helpers
{
    public class DataGridTabConstructor<T>
    {

        public required string Header { get; set; }
        public required IEnumerable<T> Data { get; init; }
    }
}
