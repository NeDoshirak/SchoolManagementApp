using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolDB.Domain.Entities
{
    public class Class
    {
        public int ClassId { get; set; }
        public int Number { get; set; }
        public string Letter { get; set; }
        public int? TeacherId { get; set; } 
        public Teacher Teacher { get; set; }

        public ICollection<Student> Students { get; set; }
        public ICollection<Schedule> Schedules { get; set; }

        public string FullName => $"{Number}{Letter}";
    }
}