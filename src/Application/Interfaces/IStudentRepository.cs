using SchoolDB.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolDB.Application.Interfaces
{
    public interface IStudentRepository
    {
        void Create(Student entity);
        Student GetById(int id);
        IEnumerable<Student> GetAll();
        void Update(Student entity);
        void Delete(int id);
    }
}
