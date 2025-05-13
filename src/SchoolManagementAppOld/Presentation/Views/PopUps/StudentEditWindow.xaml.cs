using Microsoft.Extensions.DependencyInjection;
using SchoolDB.Domain.Entities;
using SchoolManagementApp.Presentation.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SchoolManagementApp.Presentation.Views.PopUps
{
    /// <summary>
    /// Логика взаимодействия для StudentEditWindow.xaml
    /// </summary>
    public partial class StudentEditWindow : Window
    {
        private static StudentEditWindow _instance;
        private bool _isClosed;

        public static StudentEditWindow Instance
        {
            get => _instance;
            set => _instance = value;
        }

        public bool IsClosed => _isClosed;

        public StudentEditWindow(Student student)
        {
            InitializeComponent();
            _instance = this;
            _isClosed = false;
            var viewModel = App.ServiceProvider.GetRequiredService<StudentEditViewModel>();
            viewModel.Student = student;
            DataContext = viewModel;
            Closed += (s, e) => _isClosed = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
