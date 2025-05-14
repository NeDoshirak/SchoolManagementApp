using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SchoolManagementApp.Presentation.Views
{
    public partial class MainWindow : Window
    {
        private static MainWindow _instance;
        private bool _isClosed;

        public static MainWindow Instance
        {
            get => _instance;
            set => _instance = value;
        }

        public bool IsClosed => _isClosed;

        public MainWindow()
        {
            InitializeComponent();
            _instance = this;
            _isClosed = false;
            Closed += (s, e) => _isClosed = true;
        }
    }
}