using Microsoft.Extensions.DependencyInjection;
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

namespace SchoolManagementApp.Presentation.Views
{
    /// <summary>
    /// Логика взаимодействия для StudentWindow.xaml
    /// </summary>
    public partial class StudentWindow : Window
    {
        private static StudentWindow _instance;
        private bool _isClosed;

        public static StudentWindow Instance
        {
            get => _instance;
            set => _instance = value;
        }

        public bool IsClosed => _isClosed;

        public StudentWindow()
        {
            InitializeComponent();
            _instance = this;
            _isClosed = false;
            Closed += (s, e) => _isClosed = true;

            DataContext = App.ServiceProvider.GetRequiredService<StudentViewModel>();
        }

        private void StudentDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is StudentViewModel viewModel && StudentDataGrid.SelectedItem is SchoolDB.Domain.Entities.Student student)
            {
                viewModel.SelectedStudent = student;
            }
        }
    }
}
