using System;
using System.Collections.Generic;
using System.Text;

namespace School_Management.Core.Models
{
    public class Student
    {
        public int Id { get; set; }
        public DateOnly? EnrollDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        public string OtherInfo { get; set; } = string.Empty;
        public int CandidateId { get; set; }
        public Candidate Candidate { get; set; } = null!;
        public ICollection<Notification> Notifications { get; set; } = [];
        public ICollection<StudentQR> StudentQRs { get; set; } = [];
        public ICollection<StudentClass> Classes { get; set; } = [];
    }
}
