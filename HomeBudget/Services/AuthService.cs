using System.Security.Cryptography;
using System.Text;
using HomeBudget.Models;

namespace HomeBudget.Services;

public class AuthService
{
    public string HashPassword(string plainTextPassword)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(plainTextPassword));
        return Convert.ToHexString(bytes);
    }

    public bool VerifyPassword(Workspace workspace, string? providedPassword)
    {
        if (!workspace.IsProtected)
            return true;

        if (string.IsNullOrEmpty(providedPassword))
            return false;

        var hash = HashPassword(providedPassword);
        return string.Equals(workspace.PasswordHash, hash, StringComparison.OrdinalIgnoreCase);
    }
}
