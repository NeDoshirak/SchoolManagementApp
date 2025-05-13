using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolDB.Domain.Entities
{
    public class Schedule
    {
        public int ScheduleId { get; set; }
        public int ClassId { get; set; }
        public int QuarterId { get; set; }
        public int SubjectId { get; set; }
        public string DayOfWeek { get; set; }
        public int LessonNumber { get; set; }

        public Class Class { get; set; }
        public Quarter Quarter { get; set; }
        public Subject Subject { get; set; }
    }
}
