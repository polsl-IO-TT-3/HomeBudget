namespace HomeBudget.Models;

public class CategorySummaryItem
{
    public string CategoryName { get; set; } = string.Empty;
    public decimal PlannedLimit { get; set; }
    public decimal ActualSpent { get; set; }
    public double SharePercent { get; set; }
    public bool IsOverLimit => PlannedLimit > 0 && ActualSpent > PlannedLimit;
    public string PlannedLimitText => PlannedLimit > 0 ? $"{PlannedLimit:N2} zł" : "-";
    public string ActualSpentText => $"{ActualSpent:N2} zł";
    public string SharePercentText => $"{SharePercent:0}%";
}
