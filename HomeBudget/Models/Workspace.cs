namespace HomeBudget.Models;

public class Workspace
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }

    public bool IsProtected => !string.IsNullOrEmpty(PasswordHash);

    public string Initials
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Name))
                return "?";

            var parts = Name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
                return parts[0][..1].ToUpperInvariant();

            return $"{parts[0][0]}{parts[^1][0]}".ToUpperInvariant();
        }
    }
}
