using System.Windows;
using System.Windows.Controls;

namespace HomeBudget
{
    public partial class DashboardPage : Page
    {
        private readonly Gospodarstwo _gospodarstwo;

        public DashboardPage(Gospodarstwo gospodarstwo)
        {
            InitializeComponent();
            _gospodarstwo = gospodarstwo;
            NazwaGospodarstwaText.Text = gospodarstwo.Nazwa;

            Loaded += (s, e) =>
            {
                var okno = Window.GetWindow(this);
                if (okno != null) okno.Title = $"Budżet domowy — {gospodarstwo.Nazwa}";
            };
        }

        private void WylaczAplikacje_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}