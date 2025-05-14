using Microsoft.Extensions.DependencyInjection;
using Presentation.ViewModels;
using System;
using System.Windows;

namespace Presentation.Views
{
    public partial class StudentWindow : Window
    {
        private static StudentWindow _instance;
        private static readonly object _lock = new object();

        public static StudentWindow Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new StudentWindow();
                    }
                    return _instance;
                }
            }
        }

        private StudentWindow()
        {
            InitializeComponent();
            DataContext = App.Services.GetRequiredService<StudentViewModel>();
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
