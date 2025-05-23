﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Presentation.Views;
using System.Windows;

namespace Presentation.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        public MainWindowViewModel()
        {
        }

        [RelayCommand]
        private void OpenClasses()
        {
            ClassWindow.Instance.Show();
        }

        [RelayCommand]
        private void OpenStudents()
        {
            StudentWindow.Instance.Show();
        }

        [RelayCommand]
        private void OpenTeachers()
        {
            TeacherWindow.Instance.Show();
        }

        [RelayCommand]
        private void OpenSubjects()
        {
            SubjectWindow.Instance.Show();
        }

        [RelayCommand]
        private void OpenGrades()
        {
            GradeWindow.Instance.Show();
        }

        [RelayCommand]
        private void OpenQuarters()
        {
            QuarterWindow.Instance.Show();
        }

        [RelayCommand]
        private void OpenSchedules()
        {
            ScheduleWindow.Instance.Show();
        }

        [RelayCommand]
        private void About()
        {
            MessageBox.Show("Приложение для управления школой\nСоздатель: Макаров Егор\nгруппа 573-3", "О себе");
        }

        [RelayCommand]
        private void Exit()
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}