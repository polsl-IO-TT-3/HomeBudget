using System.Windows;

namespace HomeBudget
{
    public partial class LogowanieWindow : Window
    {
        private readonly Gospodarstwo _gospodarstwo;

        public LogowanieWindow(Gospodarstwo gospodarstwo)
        {
            InitializeComponent();
            _gospodarstwo = gospodarstwo;
        }

        private void Loguj_Click(object sender, RoutedEventArgs e)
        {
            if (HasloBox.Password == _gospodarstwo.Haslo)
            {
                DialogResult = true; // Zamyka pop-up i zwraca sukces
                Close();
            }
            else
            {
                MessageBox.Show("Błędne hasło!", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                HasloBox.Clear();
            }
        }
    }
}