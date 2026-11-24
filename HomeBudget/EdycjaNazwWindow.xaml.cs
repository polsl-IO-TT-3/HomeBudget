using System.Windows;

namespace HomeBudget
{
    public partial class EdycjaNazwyWindow : Window
    {
        private readonly Gospodarstwo _gospodarstwo;

        public EdycjaNazwyWindow(Gospodarstwo gospodarstwo)
        {
            InitializeComponent();
            _gospodarstwo = gospodarstwo;
            NowaNazwaBox.Text = gospodarstwo.Nazwa;
            NowaNazwaBox.SelectAll();
            NowaNazwaBox.Focus();
        }

        private void Zapisz_Click(object sender, RoutedEventArgs e)
        {
            string nowaNazwa = NowaNazwaBox.Text.Trim();
            if (string.IsNullOrEmpty(nowaNazwa))
            {
                MessageBox.Show("Nazwa nie może być pusta!", "Uwaga");
                return;
            }

            string stareHaslo = StareHasloBox.Password;
            string noweHaslo = NoweHasloBox.Password;

            // Sprawdzamy hasło tylko, jeśli użytkownik wpisał coś w polach hasła
            if (!string.IsNullOrEmpty(noweHaslo) || !string.IsNullOrEmpty(stareHaslo))
            {
                if (stareHaslo != _gospodarstwo.Haslo)
                {
                    MessageBox.Show("Obecne hasło jest nieprawidłowe!", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (string.IsNullOrEmpty(noweHaslo))
                {
                    MessageBox.Show("Wpisz nowe hasło!", "Uwaga");
                    return;
                }
                _gospodarstwo.Haslo = noweHaslo;
            }

            _gospodarstwo.Nazwa = nowaNazwa;
            DialogResult = true;
            Close();
        }
    }
}