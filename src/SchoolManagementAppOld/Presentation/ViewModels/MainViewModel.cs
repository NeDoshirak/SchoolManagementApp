using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SchoolManagementApp.Presentation.Views;
using System.Windows;

namespace SchoolManagementApp.Presentation.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [RelayCommand] 
        private void Exit()
        {
            App.ShutdownApplication();
        }

        [RelayCommand]
        private void OpenStudentWindow()
        {
            var window = StudentWindow.Instance;
            if (window == null || window.IsClosed)
            {
                window = new StudentWindow();
                StudentWindow.Instance = window;
            }
            window.Show();
        }


    }
}
