using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolDB.Domain.Entities
{
    public class Teacher
    {
        public int TeacherId { get; set; }
        public string FullName { get; set; }
        public bool IsActive { get; set; }
        public string PhotoPath { get; set; }
        public int? CabinetNumber { get; set; }
        public int? ClassId { get; set; }
        public Class Class { get; set; } 

        public ICollection<TeacherSubject> TeacherSubjects { get; set; }
    }
}
