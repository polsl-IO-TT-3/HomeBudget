namespace HomeBudget.Models;

public class Category
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Czy wyświetlać kategorię w tabelach "Limity i wydatki w kategoriach" oraz "Wszystkie wydatki [Podsumowanie]".
    /// </summary>
    public bool IsVisibleInSummary { get; set; } = true;

    /// <summary>
    /// Czy wyświetlać osobny kafelek dla tej kategorii na Panelu podglądu (na wzór Opłat stałych / Przychodu).
    /// </summary>
    public bool ShowAsSeparateCard { get; set; }
}
