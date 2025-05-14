using Microsoft.Extensions.DependencyInjection;
using Presentation.ViewModels;
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

namespace Presentation.Views
{
    /// <summary>
    /// Логика взаимодействия для TeacherWindow.xaml
    /// </summary>
    public partial class TeacherWindow : Window
    {
        private static TeacherWindow _instance;
        private static readonly object _lock = new object();

        public static TeacherWindow Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new TeacherWindow();
                    }
                    return _instance;
                }
            }
        }

        private TeacherWindow()
        {
            InitializeComponent();
            DataContext = App.Services.GetRequiredService<TeacherViewModel>();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _instance = null;
        }

        public new void Show()
        {
            if (_instance != null)
            {
                if (_instance.IsVisible)
                {
                    _instance.Activate();
                }
                else
                {
                    base.Show();
                }
            }
        }
    }
}
