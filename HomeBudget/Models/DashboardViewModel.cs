using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows.Input;
using Microsoft.Win32;

namespace HomeBudget.Models
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;

        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;
        public void Execute(object? parameter) => _execute(parameter);
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }

    public class DashboardViewModel : INotifyPropertyChanged
    {
        private static readonly CultureInfo PlCulture = new("pl-PL");
        private static readonly string[] CardColors = { "#D8CEF0", "#8FD3C7", "#F49AB8", "#FDE68A", "#BFDBFE" };

        private readonly Action _navigateLogout;
        private readonly HashSet<string> _dismissedReminders = new();
        private DateTime _selectedMonth;

        public Gospodarstwo LoggedInWorkspace { get; }

        public ObservableCollection<KategoriaBudzetu> Categories { get; } = new();
        public ObservableCollection<WidokTransakcji> MonthlyTransactions { get; } = new();
        public ObservableCollection<WidokTransakcji> MonthlyIncomes { get; } = new();
        public ObservableCollection<WidokTransakcji> RecurringExpenses { get; } = new();
        public ObservableCollection<PodsumowanieKategorii> CategorySummaries { get; } = new();
        public ObservableCollection<KafelekKategorii> CategoryCards { get; } = new();
        public ObservableCollection<PrzypomnienieItem> Reminders { get; } = new();

        public List<OpcjaCyklicznosci> RecurrenceOptions { get; } = new()
        {
            new OpcjaCyklicznosci { Value = CzestotliwoscCyklicznosci.Codziennie, Label = "Codziennie" },
            new OpcjaCyklicznosci { Value = CzestotliwoscCyklicznosci.CoTydzien, Label = "Co tydzień" },
            new OpcjaCyklicznosci { Value = CzestotliwoscCyklicznosci.CoDwaTygodnie, Label = "Co 2 tygodnie" },
            new OpcjaCyklicznosci { Value = CzestotliwoscCyklicznosci.CoMiesiac, Label = "Co miesiąc" },
            new OpcjaCyklicznosci { Value = CzestotliwoscCyklicznosci.CoKwartal, Label = "Co kwartał (co 3 mies.)" },
            new OpcjaCyklicznosci { Value = CzestotliwoscCyklicznosci.CoRok, Label = "Co roku" }
        };

        public ICommand PreviousMonthCommand { get; }
        public ICommand NextMonthCommand { get; }
        public ICommand ExportBackupCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand AddTransactionCommand { get; }
        public ICommand DeleteTransactionCommand { get; }
        public ICommand SetLimitCommand { get; }
        public ICommand AddCategoryCommand { get; }
        public ICommand DismissReminderCommand { get; }

        private string _statusMessage = "Gotowy";
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        private string _formErrorMessage = string.Empty;
        public string FormErrorMessage
        {
            get => _formErrorMessage;
            set
            {
                _formErrorMessage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasFormError));
            }
        }
        public bool HasFormError => !string.IsNullOrWhiteSpace(FormErrorMessage);

        // Nagłówek miesiąca i KPI
        public string SelectedMonthTitle => _selectedMonth.ToString("MMMM yyyy", PlCulture);
        public string PeriodRangeText
        {
            get
            {
                var start = new DateTime(_selectedMonth.Year, _selectedMonth.Month, 1);
                var end = start.AddMonths(1).AddDays(-1);
                return $"{start:dd.MM.yyyy} – {end:dd.MM.yyyy}";
            }
        }

        private string _totalIncomeText = "0,00 zł";
        public string TotalIncomeText { get => _totalIncomeText; set { _totalIncomeText = value; OnPropertyChanged(); } }

        private string _totalExpensesText = "0,00 zł";
        public string TotalExpensesText { get => _totalExpensesText; set { _totalExpensesText = value; OnPropertyChanged(); } }

        private string _remainingBudgetText = "0,00 zł";
        public string RemainingBudgetText { get => _remainingBudgetText; set { _remainingBudgetText = value; OnPropertyChanged(); } }

        private string _recurringTotalText = "0,00 zł";
        public string RecurringTotalText { get => _recurringTotalText; set { _recurringTotalText = value; OnPropertyChanged(); } }

        private double _spentRatio;
        public double SpentRatio { get => _spentRatio; set { _spentRatio = value; OnPropertyChanged(); } }

        public bool HasCategoryCards => CategoryCards.Count > 0;

        // Pola formularzy
        private WidokTransakcji? _selectedTransaction;
        public WidokTransakcji? SelectedTransaction { get => _selectedTransaction; set { _selectedTransaction = value; OnPropertyChanged(); } }

        private KategoriaBudzetu? _selectedCategory;
        public KategoriaBudzetu? SelectedCategory { get => _selectedCategory; set { _selectedCategory = value; OnPropertyChanged(); } }

        private string _newTitle = string.Empty;
        public string NewTitle
        {
            get => _newTitle;
            set { _newTitle = value; OnPropertyChanged(); ClearFormError(); }
        }

        private string _newAmountText = string.Empty;
        public string NewAmountText
        {
            get => _newAmountText;
            set { _newAmountText = value; OnPropertyChanged(); ClearFormError(); }
        }

        private DateTime _newDate = DateTime.Today;
        public DateTime NewDate
        {
            get => _newDate;
            set { _newDate = value; OnPropertyChanged(); ClearFormError(); }
        }

        private bool _isExpense = true;
        public bool IsExpense { get => _isExpense; set { _isExpense = value; OnPropertyChanged(); } }

        private bool _isRecurring;
        public bool IsRecurring { get => _isRecurring; set { _isRecurring = value; OnPropertyChanged(); } }

        private OpcjaCyklicznosci _selectedRecurrenceOption;
        public OpcjaCyklicznosci SelectedRecurrenceOption
        {
            get => _selectedRecurrenceOption;
            set { _selectedRecurrenceOption = value; OnPropertyChanged(); }
        }

        private KategoriaBudzetu? _selectedLimitCategory;
        public KategoriaBudzetu? SelectedLimitCategory
        {
            get => _selectedLimitCategory;
            set
            {
                _selectedLimitCategory = value;
                OnPropertyChanged();
                NewLimitAmountText = value != null && value.MonthlyLimit > 0 ? value.MonthlyLimit.ToString("0.##") : string.Empty;
                ClearFormError();
            }
        }

        private string _newLimitAmountText = string.Empty;
        public string NewLimitAmountText
        {
            get => _newLimitAmountText;
            set { _newLimitAmountText = value; OnPropertyChanged(); ClearFormError(); }
        }

        private string _newCategoryName = string.Empty;
        public string NewCategoryName
        {
            get => _newCategoryName;
            set { _newCategoryName = value; OnPropertyChanged(); ClearFormError(); }
        }

        public DashboardViewModel(Gospodarstwo gospodarstwo, Action navigateLogout)
        {
            LoggedInWorkspace = gospodarstwo;
            _navigateLogout = navigateLogout;
            _selectedMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            _selectedRecurrenceOption = RecurrenceOptions.First(o => o.Value == CzestotliwoscCyklicznosci.CoMiesiac);

            EnsureDefaultCategories();

            PreviousMonthCommand = new RelayCommand(_ => ChangeMonth(-1));
            NextMonthCommand = new RelayCommand(_ => ChangeMonth(1));
            ExportBackupCommand = new RelayCommand(_ => ExportJson());
            LogoutCommand = new RelayCommand(_ => _navigateLogout());
            AddTransactionCommand = new RelayCommand(_ => AddTransaction());
            DeleteTransactionCommand = new RelayCommand(_ => DeleteSelectedTransaction(), _ => SelectedTransaction != null);
            SetLimitCommand = new RelayCommand(_ => SetCategoryLimit());
            AddCategoryCommand = new RelayCommand(_ => AddCategory());
            DismissReminderCommand = new RelayCommand(param =>
            {
                if (param is PrzypomnienieItem item)
                {
                    _dismissedReminders.Add(item.ReminderKey);
                    Reminders.Remove(item);
                }
            });

            RefreshDashboard();
        }

        private void ClearFormError()
        {
            if (!string.IsNullOrEmpty(FormErrorMessage))
                FormErrorMessage = string.Empty;
        }

        private void EnsureDefaultCategories()
        {
            if (LoggedInWorkspace.Kategorie.Count == 0)
            {
                LoggedInWorkspace.Kategorie.AddRange(new[]
                {
                    new KategoriaBudzetu { Name = "Jedzenie i zakupy", MonthlyLimit = 1500, HeaderColor = "#F49AB8" },
                    new KategoriaBudzetu { Name = "Rachunki i dom", MonthlyLimit = 2000, HeaderColor = "#8FD3C7" },
                    new KategoriaBudzetu { Name = "Transport", MonthlyLimit = 500, HeaderColor = "#D8CEF0" },
                    new KategoriaBudzetu { Name = "Rozrywka", MonthlyLimit = 400, HeaderColor = "#FDE68A" },
                    new KategoriaBudzetu { Name = "Wynagrodzenie", MonthlyLimit = 0, HeaderColor = "#BFDBFE" }
                });
                BazaDanych.Zapisz();
            }

            Categories.Clear();
            foreach (var kat in LoggedInWorkspace.Kategorie)
                Categories.Add(kat);

            SelectedCategory = Categories.FirstOrDefault();
            SelectedLimitCategory = Categories.FirstOrDefault();
        }

        private void ChangeMonth(int delta)
        {
            _selectedMonth = _selectedMonth.AddMonths(delta);
            OnPropertyChanged(nameof(SelectedMonthTitle));
            OnPropertyChanged(nameof(PeriodRangeText));
            RefreshDashboard();
        }

        private IEnumerable<DateTime> GetOccurrencesInMonth(Transakcja t, DateTime monthStart, DateTime monthEnd)
        {
            DateTime start = t.Date.Date;

            if (!t.IsRecurring)
            {
                if (start >= monthStart && start <= monthEnd)
                    yield return start;
                yield break;
            }

            if (start > monthEnd)
                yield break;

            switch (t.RecurrenceInterval)
            {
                case CzestotliwoscCyklicznosci.Codziennie:
                case CzestotliwoscCyklicznosci.CoTydzien:
                case CzestotliwoscCyklicznosci.CoDwaTygodnie:
                    {
                        int stepDays = t.RecurrenceInterval switch
                        {
                            CzestotliwoscCyklicznosci.Codziennie => 1,
                            CzestotliwoscCyklicznosci.CoTydzien => 7,
                            _ => 14
                        };

                        DateTime current = start;
                        if (current < monthStart)
                        {
                            int daysDiff = (monthStart - current).Days;
                            int steps = daysDiff / stepDays;
                            current = current.AddDays(steps * stepDays);
                            if (current < monthStart)
                                current = current.AddDays(stepDays);
                        }

                        while (current <= monthEnd)
                        {
                            yield return current;
                            current = current.AddDays(stepDays);
                        }
                        break;
                    }

                case CzestotliwoscCyklicznosci.CoMiesiac:
                case CzestotliwoscCyklicznosci.CoKwartal:
                case CzestotliwoscCyklicznosci.CoRok:
                    {
                        int monthStep = t.RecurrenceInterval switch
                        {
                            CzestotliwoscCyklicznosci.CoMiesiac => 1,
                            CzestotliwoscCyklicznosci.CoKwartal => 3,
                            _ => 12
                        };

                        int monthsDiff = (monthStart.Year - start.Year) * 12 + (monthStart.Month - start.Month);
                        if (monthsDiff >= 0 && monthsDiff % monthStep == 0)
                        {
                            int day = Math.Min(start.Day, DateTime.DaysInMonth(monthStart.Year, monthStart.Month));
                            yield return new DateTime(monthStart.Year, monthStart.Month, day);
                        }
                        break;
                    }
            }
        }

        private List<WidokTransakcji> GetTransactionsForMonth(DateTime targetMonthFirstDay)
        {
            var monthEnd = targetMonthFirstDay.AddMonths(1).AddDays(-1);
            var result = new List<WidokTransakcji>();

            foreach (var t in LoggedInWorkspace.Transakcje)
            {
                foreach (var date in GetOccurrencesInMonth(t, targetMonthFirstDay, monthEnd))
                {
                    result.Add(new WidokTransakcji(t, date, OnRecurringPaidChanged));
                }
            }

            return result.OrderByDescending(x => x.Date).ToList();
        }

        private void OnRecurringPaidChanged()
        {
            BazaDanych.Zapisz();
            UpdateReminders();
            StatusMessage = "Zaktualizowano status opłaty stałej.";
        }

        public void RefreshDashboard()
        {
            var monthTransactions = GetTransactionsForMonth(_selectedMonth);

            MonthlyTransactions.Clear();
            MonthlyIncomes.Clear();
            RecurringExpenses.Clear();

            foreach (var t in monthTransactions)
            {
                MonthlyTransactions.Add(t);

                if (t.Type == TypTransakcji.Przychod)
                    MonthlyIncomes.Add(t);

                if (t.Type == TypTransakcji.Wydatek && t.IsRecurring)
                    RecurringExpenses.Add(t);
            }

            decimal totalIncome = monthTransactions.Where(t => t.Type == TypTransakcji.Przychod).Sum(t => t.Amount);
            decimal totalExpenses = monthTransactions.Where(t => t.Type == TypTransakcji.Wydatek).Sum(t => t.Amount);
            decimal remaining = totalIncome - totalExpenses;
            decimal recurringTotal = RecurringExpenses.Sum(t => t.Amount);

            TotalIncomeText = $"{totalIncome:N2} zł";
            TotalExpensesText = $"{totalExpenses:N2} zł";
            RemainingBudgetText = $"{remaining:N2} zł";
            RecurringTotalText = $"{recurringTotal:N2} zł";
            SpentRatio = totalIncome > 0 ? (double)(totalExpenses / totalIncome) : (totalExpenses > 0 ? 1.0 : 0.0);

            CategorySummaries.Clear();
            foreach (var cat in Categories.Where(c => c.IsVisibleInSummary))
            {
                decimal spent = monthTransactions
                    .Where(t => t.Type == TypTransakcji.Wydatek && t.CategoryId == cat.Id)
                    .Sum(t => t.Amount);

                if (spent == 0 && cat.MonthlyLimit == 0) continue;

                double share = totalExpenses > 0 ? (double)(spent / totalExpenses) * 100.0 : 0.0;
                CategorySummaries.Add(new PodsumowanieKategorii
                {
                    CategoryName = cat.Name,
                    PlannedLimit = cat.MonthlyLimit,
                    ActualSpent = spent,
                    SharePercent = share
                });
            }

            CategoryCards.Clear();
            foreach (var cat in Categories.Where(c => c.ShowAsSeparateCard))
            {
                var catTx = monthTransactions.Where(t => t.CategoryId == cat.Id).ToList();
                decimal sum = catTx.Sum(t => t.Amount);

                CategoryCards.Add(new KafelekKategorii
                {
                    HeaderTitle = cat.Name.ToUpper(PlCulture),
                    HeaderColor = cat.HeaderColor,
                    TotalAmountText = $"{sum:N2} zł",
                    Transactions = new ObservableCollection<WidokTransakcji>(catTx)
                });
            }
            OnPropertyChanged(nameof(HasCategoryCards));

            UpdateReminders();
        }

        private void UpdateReminders()
        {
            Reminders.Clear();

            var today = DateTime.Today;
            var horizon = today.AddDays(5);

            var monthsToCheck = new[]
            {
                new DateTime(today.Year, today.Month, 1),
                new DateTime(horizon.Year, horizon.Month, 1)
            }.Distinct();

            var upcoming = monthsToCheck
                .SelectMany(GetTransactionsForMonth)
                .Where(t => t.Type == TypTransakcji.Wydatek
                            && t.IsRecurring
                            && !t.IsPaid
                            && t.Date.Date >= today
                            && t.Date.Date <= horizon)
                .OrderBy(t => t.Date)
                .ToList();

            foreach (var item in upcoming)
            {
                string key = $"{item.Id}_{item.Date:yyyy-MM-dd}";
                if (_dismissedReminders.Contains(key)) continue;

                string whenText = item.Date.Date == today
                    ? "dzisiaj"
                    : $"dnia {item.Date:dd.MM.yyyy}";

                Reminders.Add(new PrzypomnienieItem
                {
                    ReminderKey = key,
                    Message = $"Przypomnienie o opłacie cyklicznej: „{item.Title}” — termin płatności {whenText} ({item.Amount:N2} zł)."
                });
            }
        }

        private void AddTransaction()
        {
            FormErrorMessage = string.Empty;

            if (SelectedCategory == null)
            {
                FormErrorMessage = "Wybierz kategorię z listy przed dodaniem transakcji.";
                return;
            }

            if (string.IsNullOrWhiteSpace(NewTitle))
            {
                FormErrorMessage = "Pole „Opis / tytuł” nie może być puste.";
                return;
            }

            if (string.IsNullOrWhiteSpace(NewAmountText) ||
                !decimal.TryParse(NewAmountText.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal amount) ||
                amount <= 0)
            {
                FormErrorMessage = "Wprowadź poprawną kwotę większą od zera (np. 125,50).";
                return;
            }

            var tx = new Transakcja
            {
                CategoryId = SelectedCategory.Id,
                CategoryName = SelectedCategory.Name,
                Title = NewTitle.Trim(),
                Amount = amount,
                Date = NewDate.Date,
                Type = IsExpense ? TypTransakcji.Wydatek : TypTransakcji.Przychod,
                IsRecurring = IsRecurring,
                RecurrenceInterval = SelectedRecurrenceOption?.Value ?? CzestotliwoscCyklicznosci.CoMiesiac
            };

            LoggedInWorkspace.Transakcje.Add(tx);
            BazaDanych.Zapisz();

            NewTitle = string.Empty;
            NewAmountText = string.Empty;
            NewDate = DateTime.Today;
            IsRecurring = false;
            SelectedRecurrenceOption = RecurrenceOptions.First(o => o.Value == CzestotliwoscCyklicznosci.CoMiesiac);

            RefreshDashboard();
            StatusMessage = $"Dodano transakcję: {tx.Title} ({tx.Amount:N2} zł) z datą {tx.Date:dd.MM.yyyy}.";
        }

        private void DeleteSelectedTransaction()
        {
            if (SelectedTransaction == null) return;

            string title = SelectedTransaction.Title;
            LoggedInWorkspace.Transakcje.Remove(SelectedTransaction.Source);
            BazaDanych.Zapisz();

            RefreshDashboard();
            StatusMessage = $"Usunięto transakcję: {title}.";
        }

        private void SetCategoryLimit()
        {
            FormErrorMessage = string.Empty;

            if (SelectedLimitCategory == null)
            {
                FormErrorMessage = "Wybierz kategorię, dla której chcesz ustawić limit.";
                return;
            }

            if (string.IsNullOrWhiteSpace(NewLimitAmountText) ||
                !decimal.TryParse(NewLimitAmountText.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal limit) ||
                limit < 0)
            {
                FormErrorMessage = "Wprowadź poprawną kwotę limitu (0 lub wartość dodatnią).";
                return;
            }

            SelectedLimitCategory.MonthlyLimit = limit;
            BazaDanych.Zapisz();
            RefreshDashboard();
            StatusMessage = $"Ustawiono limit {limit:N2} zł dla kategorii „{SelectedLimitCategory.Name}”.";
        }

        private void AddCategory()
        {
            FormErrorMessage = string.Empty;

            string name = NewCategoryName.Trim();
            if (string.IsNullOrEmpty(name))
            {
                FormErrorMessage = "Wpisz nazwę nowej kategorii.";
                return;
            }

            if (Categories.Any(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase)))
            {
                FormErrorMessage = $"Kategoria „{name}” już istnieje na liście.";
                return;
            }

            var kat = new KategoriaBudzetu
            {
                Name = name,
                HeaderColor = CardColors[Categories.Count % CardColors.Length]
            };

            LoggedInWorkspace.Kategorie.Add(kat);
            Categories.Add(kat);
            SelectedCategory = kat;
            SelectedLimitCategory = kat;
            NewCategoryName = string.Empty;

            BazaDanych.Zapisz();
            RefreshDashboard();
            StatusMessage = $"Utworzono nową kategorię: „{name}”.";
        }

        private void ExportJson()
        {
            var dialog = new SaveFileDialog
            {
                FileName = $"Budzet_{LoggedInWorkspace.Nazwa}_{_selectedMonth:yyyy_MM}.json",
                Filter = "Plik JSON (*.json)|*.json"
            };

            if (dialog.ShowDialog() == true)
            {
                string json = JsonSerializer.Serialize(LoggedInWorkspace, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(dialog.FileName, json);
                StatusMessage = $"Wyeksportowano dane do pliku {Path.GetFileName(dialog.FileName)}.";
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? prop = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}