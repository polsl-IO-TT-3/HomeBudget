using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using HomeBudget.Models;

namespace HomeBudget
{
    public static class HasloHelper
    {
        public static string Hash(string tekst) =>
            string.IsNullOrEmpty(tekst)
                ? string.Empty
                : Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(tekst)));
    }

    public class Gospodarstwo : INotifyPropertyChanged
    {
        public List<KategoriaBudzetu> Kategorie { get; set; } = new();
        public List<Transakcja> Transakcje { get; set; } = new();
        private string _nazwa = string.Empty;
        public string Nazwa
        {
            get => _nazwa;
            set
            {
                _nazwa = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Nazwa)));
            }
        }

        private string _inicjal = "?";
        [JsonIgnore]
        public string Inicjal
        {
            get => _inicjal;
            set
            {
                if (_inicjal == value) return;
                _inicjal = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Inicjal)));
            }
        }

        private string _hasloHash = string.Empty;
        public string HasloHash
        {
            get => _hasloHash;
            set
            {
                _hasloHash = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasloHash)));
            }
        }

        public void UstawHaslo(string noweHaslo) => HasloHash = HasloHelper.Hash(noweHaslo);

        public bool SprawdzHaslo(string wpisaneHaslo) =>
            string.IsNullOrEmpty(HasloHash) || HasloHelper.Hash(wpisaneHaslo) == HasloHash;

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    public static class BazaDanych
    {
        private static readonly string SciezkaPliku = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "HomeBudget",
            "gospodarstwa.json");

        public static ObservableCollection<Gospodarstwo> Gospodarstwa { get; } = Wczytaj();

        static BazaDanych()
        {
            OdswiezInicjaly();
        }

        public static bool CzyNazwaIstnieje(string nazwa, Gospodarstwo? pomijane = null)
        {
            return Gospodarstwa.Any(g =>
                g != pomijane &&
                string.Equals(g.Nazwa.Trim(), nazwa.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        public static void OdswiezInicjaly()
        {
            var uzyte = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var g in Gospodarstwa)
            {
                string nazwa = g.Nazwa?.Trim() ?? string.Empty;
                if (string.IsNullOrEmpty(nazwa))
                {
                    g.Inicjal = "?";
                    continue;
                }

                var czesci = nazwa.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                // 1. Bazowy inicjał (1 litera dla jednego słowa, 2 litery dla wielu słów)
                string kandydat = czesci.Length >= 2
                    ? $"{char.ToUpper(czesci[0][0])}{char.ToUpper(czesci[1][0])}"
                    : char.ToUpper(czesci[0][0]).ToString();

                // 2. Jeśli zajęty i pierwsze słowo ma min. 2 litery -> weź dwie pierwsze litery (np. S -> SY)
                if (uzyte.Contains(kandydat) && czesci[0].Length >= 2)
                {
                    kandydat = $"{char.ToUpper(czesci[0][0])}{char.ToUpper(czesci[0][1])}";
                }

                // 3. Jeśli nadal zajęty -> dopisz cyfrę (S1, S2...)
                if (uzyte.Contains(kandydat))
                {
                    char pierwsza = char.ToUpper(czesci[0][0]);
                    int numer = 1;
                    do
                    {
                        kandydat = $"{pierwsza}{numer}";
                        numer++;
                    } while (uzyte.Contains(kandydat));
                }

                uzyte.Add(kandydat);
                g.Inicjal = kandydat;
            }
        }

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
            OdswiezInicjaly();

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