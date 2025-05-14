using Application.Interfaces;
using SchoolDB.Application.Interfaces;
using System;

namespace Infrastructure.Services
{
    public class DataChangeNotifier : IDataChangeNotifier
    {
        public event Action ClassChanged;
        public event Action StudentChanged;
        public event Action TeacherChanged;
        public event Action SubjectChanged;
        public event Action GradeChanged;
        public event Action QuarterChanged;
        public event Action ScheduleChanged;
        public event Action TeacherSubjectChanged;

        public void NotifyClassChanged() => ClassChanged?.Invoke();
        public void NotifyStudentChanged() => StudentChanged?.Invoke();
        public void NotifyTeacherChanged() => TeacherChanged?.Invoke();
        public void NotifySubjectChanged() => SubjectChanged?.Invoke();
        public void NotifyGradeChanged() => GradeChanged?.Invoke();
        public void NotifyQuarterChanged() => QuarterChanged?.Invoke();
        public void NotifyScheduleChanged() => ScheduleChanged?.Invoke();
        public void NotifyTeacherSubjectChanged() => TeacherSubjectChanged?.Invoke();
    }
}