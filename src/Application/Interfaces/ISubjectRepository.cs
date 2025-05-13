using SchoolDB.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolDB.Application.Interfaces
{
    public interface ISubjectRepository
    {
        void Create(Subject entity);
        Subject GetById(int id);
        IEnumerable<Subject> GetAll();
        void Update(Subject entity);
        void Delete(int id);
    }
}
