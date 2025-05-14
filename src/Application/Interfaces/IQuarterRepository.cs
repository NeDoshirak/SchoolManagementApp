using SchoolDB.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolDB.Application.Interfaces
{
    public interface IQuarterRepository
    {
        void Create(Quarter entity);
        Quarter GetById(int id);
        IEnumerable<Quarter> GetAll();
        void Update(Quarter entity);
        void Delete(int id);
    }
}
