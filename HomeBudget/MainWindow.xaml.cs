using System.Windows;

namespace HomeBudget
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new ListaGospodarstwPage());
        }
    }
}
