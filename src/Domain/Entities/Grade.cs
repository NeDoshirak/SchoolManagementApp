using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolDB.Domain.Entities
{
    public class Grade
    {
        public int GradeId { get; set; }
        public int StudentId { get; set; }
        public int SubjectId { get; set; }
        public int QuarterId { get; set; }
        public int GradeValue { get; set; }

        public Student Student { get; set; }
        public Subject Subject { get; set; }
        public Quarter Quarter { get; set; }
    }
}
