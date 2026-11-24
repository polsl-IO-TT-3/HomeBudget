using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace HomeBudget
{
 
    public class HasloHelper
    {
        public static string Hash(string tekst) =>
            string.IsNullOrEmpty(tekst)
                ? string.Empty
                : Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(tekst)));
    }
    public class Gospodarstwo : INotifyPropertyChanged
    {
        private string _nazwa = string.Empty;
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
        public string Inicjal
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Nazwa)) return "?";
                var czesci = Nazwa.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (czesci.Length >= 2)
                {
                    return $"{char.ToUpper(czesci[0][0])}{char.ToUpper(czesci[1][0])}";
                }
                return char.ToUpper(czesci[0][0]).ToString();
            }
        }

        private string _haslo = string.Empty;
        public string Haslo
        {
            get => _haslo;
            set
            {
                _haslo = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Haslo)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CzyChronione)));
            }
        }

        [JsonIgnore]
        public bool CzyChronione => !string.IsNullOrEmpty(Haslo);

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    public static class BazaDanych
    {
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
                catch
                {
                    // W razie uszkodzonego pliku załaduje domyślne
                }
            }

            return new ObservableCollection<Gospodarstwo>();
        }

        public static void Zapisz()
        {
            string? katalog = Path.GetDirectoryName(SciezkaPliku);
            if (!string.IsNullOrEmpty(katalog))
            {
                Directory.CreateDirectory(katalog);
            }
            string json = JsonSerializer.Serialize(Gospodarstwa, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SciezkaPliku, json);
        }
    }
}
