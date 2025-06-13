using System.Security.Cryptography;
using System.Text;

namespace idcc.Helper;

public class PasswordHelper
{
    public static string Hash(string input)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}