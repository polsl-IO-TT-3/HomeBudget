using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HomeBudget
{
    public class Gospodarstwo : INotifyPropertyChanged
    {
        private string _nazwa;
        public string Nazwa
        {
            get => _nazwa;
            set
            {
                _nazwa = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Nazwa)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Inicjal)));
            }
        }

        [JsonIgnore]
        public string Inicjal => string.IsNullOrWhiteSpace(Nazwa) ? "?" : Nazwa.Trim()[0].ToString().ToUpper();

        public string Haslo { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    public static class BazaDanych
    {
        // Zapisujemy w AppData, żeby "Wyczyść rozwiązanie" w Visual Studio nie usuwało pliku
        private static readonly string SciezkaPliku = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "HomeBudget",
            "gospodarstwa.json");

        public static ObservableCollection<Gospodarstwo> Gospodarstwa { get; } = Wczytaj();

        private static ObservableCollection<Gospodarstwo> Wczytaj()
        {
            if (File.Exists(SciezkaPliku))
            {
                try
                {
                    string json = File.ReadAllText(SciezkaPliku);
                    var lista = JsonSerializer.Deserialize<ObservableCollection<Gospodarstwo>>(json);
                    if (lista != null) return lista;
                }
                catch { /* W razie uszkodzonego pliku załaduje domyślne */ }
            }

            // Domyślne dane przy pierwszym uruchomieniu
            return new ObservableCollection<Gospodarstwo>
            {
                new Gospodarstwo { Nazwa = "SzopaPodolsza", Haslo = "Kacper" },
                new Gospodarstwo { Nazwa = "SzopaBędzin", Haslo = "Michal" }
            };
        }

        public static void Zapisz()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SciezkaPliku));
            string json = JsonSerializer.Serialize(Gospodarstwa, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SciezkaPliku, json);
        }
    }
}