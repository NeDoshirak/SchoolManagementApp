using Application.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SchoolDB.Application.Interfaces;
using SchoolDB.Domain.Entities;
using SchoolManagementApp.Presentation.Views.PopUps;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagementApp.Presentation.ViewModels
{
    public partial class StudentViewModel : ObservableObject
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IDataChangeNotifier _dataChangeNotifier;

        [ObservableProperty]
        private ObservableCollection<Student> students;

        [ObservableProperty]
        private Student selectedStudent;

        public StudentViewModel(IStudentRepository studentRepository, IDataChangeNotifier dataChangeNotifier)
        {
            _studentRepository = studentRepository;
            _dataChangeNotifier = dataChangeNotifier;
            Students = new ObservableCollection<Student>(_studentRepository.GetAll());
            _dataChangeNotifier.StudentChanged += () => Students = new ObservableCollection<Student>(_studentRepository.GetAll());
        }

        [RelayCommand]
        private void Delete(int studentId)
        {
            var student = Students.FirstOrDefault(s => s.StudentId == studentId);
            if (student != null)
            {
                _studentRepository.Delete(studentId);
                Students.Remove(student);
                _dataChangeNotifier.NotifyStudentChanged();
            }
        }

        [RelayCommand]
        private void Edit(int studentId)
        {
            var studentToEdit = Students.FirstOrDefault(s => s.StudentId == studentId);
            if (studentToEdit != null)
            {
                var editWindow = new StudentEditWindow(studentToEdit);
                editWindow.ShowDialog();
            }
        }
    }
}
