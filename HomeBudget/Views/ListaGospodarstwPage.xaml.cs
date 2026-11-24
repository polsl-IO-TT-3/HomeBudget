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
            Loaded += (s, e) =>
            {
                var okno = Window.GetWindow(this);
                if (okno != null) okno.Title = "Budżet domowy";
            };
        }

        private void DodajGospodarstwo_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new TworzenieGospodarstwaPage());
        }
    }
}