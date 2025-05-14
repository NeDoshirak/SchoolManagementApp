using Presentation.ViewModels;
using SchoolDB.Domain.Entities;
using System.Windows;

namespace Presentation.Views.Popups
{
    public partial class StudentEditPopup : Window
    {
        public StudentEditPopup(Student student)
        {
            InitializeComponent();
            DataContext = new StudentEditViewModel(student, this);
        }
    }
}
