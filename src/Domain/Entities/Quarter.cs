using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolDB.Domain.Entities
{
    public class Quarter
    {
        public int QuarterId { get; set; }
        public int QuarterNumber { get; set; }
        public string AcademicYear { get; set; }

        public ICollection<Schedule> Schedules { get; set; }
        public ICollection<Grade> Grades { get; set; }
    }
}
