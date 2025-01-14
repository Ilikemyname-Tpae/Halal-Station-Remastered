using Halal_Station_Remastered.Utils.Authorization;
using Halal_Station_Remastered.Utils.JTW;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

public class JWTGenerator
{
    private static readonly string SecretKey = "n9stoFfd/JN6JyVCxwEXYxNSXGDEGSoOcPtd7erDtE4";
    private static readonly string Issuer = "HaloOnlineServer";
    private static readonly string Audience = "HaloOnlineUser";

    public static string GenerateJwtToken(string userId, string username)
    {
        var header = new
        {
            typ = "JWT",
            alg = "HS256"
        };

        var payload = new
        {
            id = userId,
            username,
            iss = Issuer,
            aud = Audience,
            exp = UnixTime.GetNow() + 30 * 60,
            nbf = UnixTime.GetNow(),
        };

        var headerJson = JsonSerializer.Serialize(header);
        var payloadJson = JsonSerializer.Serialize(payload);
        var headerBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(headerJson))
                            .TrimEnd('=')
                            .Replace('+', '-')
                            .Replace('/', '_');
        var payloadBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(payloadJson))
                             .TrimEnd('=')
                             .Replace('+', '-')
                             .Replace('/', '_');
        var signature = SignatureGenerator.CreateSignature(headerBase64, payloadBase64);

        return $"{headerBase64}.{payloadBase64}.{signature}";
    }

    public static ClaimsPrincipal GetPrincipalFromToken(string token)
    {
        if (string.IsNullOrEmpty(token))
            throw new ArgumentException("Token is null or empty.");

        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = Issuer,
            ValidAudience = Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey))
        };

        try
        {
            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            return principal;
        }
        catch
        {
            return null;
        }
    }

    public static string GetUserIdFromToken(string token)
    {
        var principal = GetPrincipalFromToken(token);
        if (principal == null)
            return null;

        var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == "id");
        return userIdClaim?.Value;
    }
}