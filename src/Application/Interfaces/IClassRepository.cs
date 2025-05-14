using SchoolDB.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolDB.Application.Interfaces
{
    public interface IClassRepository
    {
        void Create(Class entity);
        Class GetById(int id);
        IEnumerable<Class> GetAll();
        void Update(Class entity);
        void Delete(int id);
    }
}
