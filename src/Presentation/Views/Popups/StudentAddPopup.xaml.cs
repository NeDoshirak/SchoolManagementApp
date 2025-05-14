using Presentation.ViewModels;
using System.Windows;

namespace Presentation.Views.Popups
{
    public partial class StudentAddPopup : Window
    {
        public StudentAddPopup(int classId)
        {
            InitializeComponent();
            DataContext = new StudentAddViewModel(classId, this);
        }
    }
}
