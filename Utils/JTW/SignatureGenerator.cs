using System.Security.Cryptography;
using System.Text;

namespace Halal_Station_Remastered.Utils.JTW
{
    public class SignatureGenerator
    {
        private const string SecretKey = "n9stoFfd/JN6JyVCxwEXYxNSXGDEGSoOcPtd7erDtE4";
        public static string CreateSignature(string headerBase64, string payloadBase64)
        {
            var keyBytes = Encoding.UTF8.GetBytes(SecretKey);
            var dataToSign = $"{headerBase64}.{payloadBase64}";

            using (var hmac = new HMACSHA256(keyBytes))
            {
                var signatureBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(dataToSign));
                return Convert.ToBase64String(signatureBytes)
                        .TrimEnd('=')
                        .Replace('+', '-')
                        .Replace('/', '_');
            }
        }
    }
}