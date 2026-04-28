using System.Security.Cryptography;
using System.Text;

namespace IdentityService.Api.Services;

public class RefreshTokenHasher
{
    public string Hash(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }
}
