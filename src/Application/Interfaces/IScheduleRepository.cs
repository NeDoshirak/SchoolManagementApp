using SchoolDB.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolDB.Application.Interfaces
{
    public interface IScheduleRepository
    {
        void Create(Schedule entity);
        Schedule GetById(int id);
        IEnumerable<Schedule> GetAll();
        void Update(Schedule entity);
        void Delete(int id);
    }
}
