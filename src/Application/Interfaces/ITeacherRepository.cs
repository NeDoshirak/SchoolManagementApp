using SchoolDB.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolDB.Application.Interfaces
{
    public interface ITeacherRepository
    {
        void Create(Teacher entity);
        Teacher GetById(int id);
        IEnumerable<Teacher> GetAll();
        void Update(Teacher entity);
        void Delete(int id);
    }
}
