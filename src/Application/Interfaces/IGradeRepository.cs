using SchoolDB.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolDB.Application.Interfaces
{
    public interface IGradeRepository
    {
        void Create(Grade entity);
        Grade GetById(int id);
        IEnumerable<Grade> GetAll();
        void Update(Grade entity);
        void Delete(int id);
        IEnumerable<Grade> GetByStudentId(int studentId);
    }
}
