using SchoolDB.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolDB.Application.Interfaces
{
    public interface ITeacherSubjectRepository
    {
        void Create(TeacherSubject entity);
        IEnumerable<TeacherSubject> GetAll();
        void Delete(int teacherId, int subjectId);
    }
}
