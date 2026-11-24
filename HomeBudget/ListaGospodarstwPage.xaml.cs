using System.Windows;
using System.Windows.Controls;

namespace HomeBudget
{
    public partial class ListaGospodarstwPage : Page
    {
        public ListaGospodarstwPage()
        {
            InitializeComponent();
            ListaGospodarstw.ItemsSource = BazaDanych.Gospodarstwa;
        }

        private void Gospodarstwo_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button przycisk && przycisk.Tag is Gospodarstwo wybraneGospodarstwo)
            {
                var oknoLogowania = new LogowanieWindow(wybraneGospodarstwo)
                {
                    Owner = Window.GetWindow(this)
                };

                if (oknoLogowania.ShowDialog() == true)
                {
                    MessageBox.Show($"Zalogowano do: {wybraneGospodarstwo.Nazwa}!", "Sukces");
                }
            }
        }

        private void EdytujNazwe_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button przycisk && przycisk.Tag is Gospodarstwo wybraneGospodarstwo)
            {
                var oknoEdycji = new EdycjaNazwyWindow(wybraneGospodarstwo)
                {
                    Owner = Window.GetWindow(this)
                };

                if (oknoEdycji.ShowDialog() == true)
                {
                    BazaDanych.Zapisz();
                }
            }
        }

        private void UsunGospodarstwo_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button przycisk && przycisk.Tag is Gospodarstwo wybraneGospodarstwo)
            {
                var oknoPotwierdzenia = new LogowanieWindow(wybraneGospodarstwo)
                {
                    Title = "Potwierdź usunięcie",
                    Owner = Window.GetWindow(this)
                };

                if (oknoPotwierdzenia.ShowDialog() == true)
                {
                    BazaDanych.Gospodarstwa.Remove(wybraneGospodarstwo);
                    BazaDanych.Zapisz();
                }
            }
        }

        private void DodajGospodarstwo_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new TworzenieGospodarstwaPage());
        }
    }
}
