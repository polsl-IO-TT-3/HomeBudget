namespace HomeBudget.Models;

public class Transaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkspaceId { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
    public bool IsRecurring { get; set; }
    public int? BillingDay { get; set; }
    public bool IsPaid { get; set; } = true;

    public decimal SignedAmount => Type == TransactionType.Income ? Amount : -Amount;
    public string TypeLabel => Type == TransactionType.Income ? "Przychód" : "Wydatek";
    public string BillingDayText => BillingDay.HasValue ? BillingDay.Value.ToString("00") : "-";
}
