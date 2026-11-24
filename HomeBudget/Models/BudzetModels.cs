using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace HomeBudget.Models
{
    public enum TypTransakcji
    {
        Wydatek,
        Przychod
    }

    public enum CzestotliwoscCyklicznosci
    {
        Codziennie,
        CoTydzien,
        CoDwaTygodnie,
        CoMiesiac,
        CoKwartal,
        CoRok
    }

    public class OpcjaCyklicznosci
    {
        public CzestotliwoscCyklicznosci Value { get; set; }
        public string Label { get; set; } = string.Empty;
    }

    public class KategoriaBudzetu : INotifyPropertyChanged
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        private decimal _monthlyLimit;
        public decimal MonthlyLimit
        {
            get => _monthlyLimit;
            set { _monthlyLimit = value; OnPropertyChanged(); }
        }

        public string HeaderColor { get; set; } = "#D8CEF0";

        private bool _isVisibleInSummary = true;
        public bool IsVisibleInSummary
        {
            get => _isVisibleInSummary;
            set { _isVisibleInSummary = value; OnPropertyChanged(); }
        }

        private bool _showAsSeparateCard;
        public bool ShowAsSeparateCard
        {
            get => _showAsSeparateCard;
            set { _showAsSeparateCard = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? prop = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    public class Transakcja
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; } = DateTime.Today;
        public TypTransakcji Type { get; set; } = TypTransakcji.Wydatek;
        public bool IsRecurring { get; set; }
        public CzestotliwoscCyklicznosci RecurrenceInterval { get; set; } = CzestotliwoscCyklicznosci.CoMiesiac;

        // Lista konkretnych dat wystąpień w formacie "yyyy-MM-dd", które zostały opłacone
        public HashSet<string> PaidDates { get; set; } = new();
    }

    public class WidokTransakcji : INotifyPropertyChanged
    {
        private readonly string _occurrenceKey;
        private readonly Action? _onPaidChanged;

        public Transakcja Source { get; }
        public DateTime Date { get; }

        public WidokTransakcji(Transakcja source, DateTime displayDate, Action? onPaidChanged = null)
        {
            Source = source;
            Date = displayDate;
            _occurrenceKey = displayDate.ToString("yyyy-MM-dd");
            _onPaidChanged = onPaidChanged;
        }

        public Guid Id => Source.Id;
        public Guid CategoryId => Source.CategoryId;
        public string CategoryName => Source.CategoryName;
        public string Title => Source.Title;
        public decimal Amount => Source.Amount;
        public TypTransakcji Type => Source.Type;
        public bool IsRecurring => Source.IsRecurring;
        public string TypeLabel => Source.Type == TypTransakcji.Przychod ? "Przychód" : "Wydatek";

        public bool IsPaid
        {
            get => Source.PaidDates.Contains(_occurrenceKey);
            set
            {
                bool changed = value
                    ? Source.PaidDates.Add(_occurrenceKey)
                    : Source.PaidDates.Remove(_occurrenceKey);

                if (changed)
                {
                    OnPropertyChanged();
                    _onPaidChanged?.Invoke();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? prop = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    public class PrzypomnienieItem
    {
        public string ReminderKey { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class PodsumowanieKategorii
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal PlannedLimit { get; set; }
        public decimal ActualSpent { get; set; }
        public double SharePercent { get; set; }

        public string PlannedLimitText => PlannedLimit > 0 ? $"{PlannedLimit:N2} zł" : "Brak limitu";
        public string ActualSpentText => $"{ActualSpent:N2} zł";
        public string SharePercentText => $"{SharePercent:0.#}%";
    }

    public class KafelekKategorii
    {
        public string HeaderTitle { get; set; } = string.Empty;
        public string HeaderColor { get; set; } = "#D8CEF0";
        public string TotalAmountText { get; set; } = "0,00 zł";
        public ObservableCollection<WidokTransakcji> Transactions { get; set; } = new();
    }
}