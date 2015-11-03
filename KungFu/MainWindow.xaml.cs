using System.Reflection;
using System.Windows;

namespace KungFu
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Version.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}
