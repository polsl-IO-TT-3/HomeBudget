using System.Windows;

namespace HomeBudget
{
    public partial class UsunGospodarstwoWindow : Window
    {
        private readonly Gospodarstwo _gospodarstwo;

        public UsunGospodarstwoWindow(Gospodarstwo gospodarstwo)
        {
            InitializeComponent();
            _gospodarstwo = gospodarstwo;
            OpisText.Text = $"Czy na pewno chcesz trwale usunąć „{gospodarstwo.Nazwa}”?";

            if (_gospodarstwo.CzyChronione)
            {
                HasloPanel.Visibility = Visibility.Visible;
                HasloBox.Focus();
            }
            else
            {
                HasloPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void Usun_Click(object sender, RoutedEventArgs e)
        {
            if (_gospodarstwo.CzyChronione && !_gospodarstwo.SprawdzHaslo(HasloBox.Password))
            {
                ErrorText.Text = "Nieprawidłowe hasło. Gospodarstwo nie zostało usunięte.";
                HasloBox.Clear();
                HasloBox.Focus();
                return;
            }

            DialogResult = true;
            Close();
        }
    }
}