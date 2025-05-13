using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolDB.Domain.Entities
{
    public class Subject
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; }

        public ICollection<Schedule> Schedules { get; set; }
        public ICollection<Grade> Grades { get; set; }
        public ICollection<TeacherSubject> TeacherSubjects { get; set; }
    }
}
