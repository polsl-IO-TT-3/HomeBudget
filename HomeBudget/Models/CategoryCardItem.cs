namespace HomeBudget.Models;

public class CategoryCardItem
{
    public string CategoryName { get; set; } = string.Empty;
    public string HeaderTitle => CategoryName.ToUpperInvariant();
    public string HeaderColor { get; set; } = "#8FD3C7";
    public List<Transaction> Transactions { get; set; } = new();

    // Bilans kategorii: przychody (+) minus wydatki (-)
    public decimal TotalAmount => Transactions.Sum(t => t.SignedAmount);

    public string TotalAmountText => TotalAmount > 0
        ? $"+{TotalAmount:N2} zł"
        : $"{TotalAmount:N2} zł";
}
