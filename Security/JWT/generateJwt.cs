using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace DeveSecurity
{
    partial class Auth
    {
        public static string GenerateAccessToken(GetUserDto data)
        {
            Claim[] claims =
            [
                new Claim(JwtRegisteredClaimNames.Sub, data.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, data.Name),
                new Claim("login", data.Login)
            ];

            string secretKey = "34u584ngwejg-324252001143576845m";
            var ketBytes = Encoding.UTF8.GetBytes(secretKey);
            var securityKey = new SymmetricSecurityKey(ketBytes);
            var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var accessToken = new JwtSecurityToken
            (
                issuer: "DEVE",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(accessToken);
        }

        public static string GenerateRefreshToken(GetUserDto data)
        {
            string secretKey = "34u584ngwejg-324252001143576845m";
            var ketBytes = Encoding.UTF8.GetBytes(secretKey);
            var securityKey = new SymmetricSecurityKey(ketBytes);
            var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var refreshToken = new JwtSecurityToken
            (
                issuer: "DEVE",
                claims: [new Claim(JwtRegisteredClaimNames.Sub, data.UserId.ToString())],
                expires: DateTime.UtcNow.AddMonths(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(refreshToken);
        }

        public static GetUserDto DecodeToken(string token)
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            var jwt = jwtHandler.ReadJwtToken(token);

            return new GetUserDto(){UserId = Guid.Parse(jwt.Payload.Sub), Login = jwt.Claims.FirstOrDefault(c => c.Type == "login")?.Value!, Name = jwt.Claims.FirstOrDefault(c => c.Type == "name")?.Value!};
        }

        public static string HashToken(string token)
        {
            return BCrypt.Net.BCrypt.HashPassword(token);
        }

        public static bool VerifyTokenHashs(string token, string tokenHash)
        {
            return BCrypt.Net.BCrypt.Verify(token, tokenHash);
        }
    }
}