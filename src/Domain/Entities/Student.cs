using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolDB.Domain.Entities
{
    public class Student
    {
        public int StudentId { get; set; }
        public string FullName { get; set; }
        public int ClassId { get; set; }
        public string PhotoPath { get; set; }

        public Class Class { get; set; }
        public ICollection<Grade> Grades { get; set; }
        public double? AverageGrade { get; set; }


    }
}
