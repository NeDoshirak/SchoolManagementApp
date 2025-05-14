using Application.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Presentation.Views.Popups;
using SchoolDB.Application.Interfaces;
using SchoolDB.Domain.Entities;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace Presentation.ViewModels
{
    public partial class StudentViewModel : ObservableObject
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IGradeRepository _gradeRepository;
        private readonly IDataChangeNotifier _dataChangeNotifier;

        [ObservableProperty]
        private ObservableCollection<Student> _students;

        [ObservableProperty]
        private Student _selectedStudent;

        [ObservableProperty]
        private ObservableCollection<string> _selectedStudentGrades;

        public bool HasSelectedStudent => SelectedStudent != null;

        public StudentViewModel(IStudentRepository studentRepository, IGradeRepository gradeRepository, IDataChangeNotifier dataChangeNotifier)
        {
            _studentRepository = studentRepository;
            _gradeRepository = gradeRepository;
            _dataChangeNotifier = dataChangeNotifier;

            LoadStudents();
            SubscribeToChanges();

            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(SelectedStudent))
                {
                    UpdateGrades();
                    OnPropertyChanged(nameof(HasSelectedStudent));
                }
            };
        }

        private void LoadStudents()
        {
            Students = new ObservableCollection<Student>(_studentRepository.GetAll());
        }

        private void UpdateGrades()
        {
            if (SelectedStudent == null)
            {
                SelectedStudentGrades = null;
                return;
            }

            var grades = _gradeRepository.GetByStudentId(SelectedStudent.StudentId);
            SelectedStudentGrades = new ObservableCollection<string>(
                grades.Select(g => $"{g.Subject.SubjectName} - {g.GradeValue}"));
        }

        private void SubscribeToChanges()
        {
            _dataChangeNotifier.StudentChanged += () =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    LoadStudents();
                    if (SelectedStudent != null)
                    {
                        SelectedStudent = Students.FirstOrDefault(st => st.StudentId == SelectedStudent.StudentId);
                    }
                });
            };
        }

        [RelayCommand]
        private void AddStudent()
        {
            var editPopup = new StudentEditPopup(null);
            editPopup.ShowDialog();
        }

        [RelayCommand]
        private void EditStudent(Student student)
        {
            if (student == null) return;
            var editPopup = new StudentEditPopup(student);
            editPopup.ShowDialog();
        }

        [RelayCommand]
        private void DeleteStudent(Student student)
        {
            if (student == null) return;
            if (MessageBox.Show($"Вы уверены, что хотите удалить ученика {student.FullName}?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    _studentRepository.Delete(student.StudentId);
                    _dataChangeNotifier.NotifyStudentChanged();
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}