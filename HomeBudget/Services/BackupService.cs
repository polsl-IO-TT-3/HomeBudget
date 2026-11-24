using System.IO;
using System.Text.Json;
using HomeBudget.DataAccess;

namespace HomeBudget.Services;

public class BackupService
{
    private readonly InMemoryDataStore _store;

    public BackupService(InMemoryDataStore store)
    {
        _store = store;
    }

    public string ExportBackup(string? customDirectory = null)
    {
        var folder = customDirectory ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups");
        Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, $"HomeBudget_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.json");

        var snapshot = new
        {
            ExportedAt = DateTime.Now,
            _store.Workspaces,
            _store.Categories,
            _store.Transactions,
            _store.BudgetLimits
        };

        var json = JsonSerializer.Serialize(snapshot, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json);
        return path;
    }
}
