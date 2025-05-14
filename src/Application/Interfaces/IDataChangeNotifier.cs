using System;

namespace Application.Interfaces
{
    public interface IDataChangeNotifier
    {
        event Action ClassChanged;
        event Action StudentChanged;
        event Action TeacherChanged;
        event Action SubjectChanged;
        event Action GradeChanged;
        event Action QuarterChanged;
        event Action ScheduleChanged;
        event Action TeacherSubjectChanged;

        void NotifyClassChanged();
        void NotifyStudentChanged();
        void NotifyTeacherChanged();
        void NotifySubjectChanged();
        void NotifyGradeChanged();
        void NotifyQuarterChanged();
        void NotifyScheduleChanged();
        void NotifyTeacherSubjectChanged();
    }
}