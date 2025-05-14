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
    /// Логика взаимодействия для SubjectWindow.xaml
    /// </summary>
    public partial class SubjectWindow : Window
    {
        private static SubjectWindow _instance;
        private static readonly object _lock = new object();

        public static SubjectWindow Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new SubjectWindow();
                    }
                    return _instance;
                }
            }
        }

        private SubjectWindow()
        {
            InitializeComponent();
            DataContext = App.Services.GetRequiredService<SubjectViewModel>();
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
