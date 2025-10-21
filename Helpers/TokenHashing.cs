using System.Security.Cryptography;
using System.Text;

namespace fuszerkomat_api.Helpers
{
    public static class TokenHashing
    {
        public static string Hash(string rawToken, string secret)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(rawToken)));
        }
        public static string NewOpaqueTokenBase64()
        {
            var bytes = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(bytes);
        }
    }
}
