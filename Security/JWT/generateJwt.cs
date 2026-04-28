using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace DeveSecurity
{
    public interface IAuth
    {
        public string GenerateAccessToken(GetUserDto data);
        public string GenerateRefreshToken(GetUserDto data);
        public GetUserDto DecodeToken(string token);
        public string HashToken(string token);
        public bool VerifyTokenHashs(string token, string tokenHash);
    }

    partial class Auth : IAuth
    {
        private readonly IConfiguration _configuration;
        private readonly string _secretKey;
        private readonly string _issuer;
        public Auth(IConfiguration configuration)
        {
            _configuration = configuration;

            _secretKey = _configuration["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("Jwt SecretKey was not provided");
            _issuer = _configuration["JwtSettings:Issuer"] ?? throw new InvalidOperationException("Jwt Issuer was not provided");
        }

        public string GenerateAccessToken(GetUserDto data)
        {
            Claim[] claims =
            [
                new Claim(JwtRegisteredClaimNames.Sub, data.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, data.Name),
                new Claim("login", data.Login)
            ];

            var keyBytes = Encoding.UTF8.GetBytes(_secretKey);
            var securityKey = new SymmetricSecurityKey(keyBytes);
            var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var accessToken = new JwtSecurityToken
            (
                issuer: _issuer,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(accessToken);
        }

        public string GenerateRefreshToken(GetUserDto data)
        {
            var keyBytes = Encoding.UTF8.GetBytes(_secretKey);
            var securityKey = new SymmetricSecurityKey(keyBytes);
            var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var refreshToken = new JwtSecurityToken
            (
                issuer: _issuer,
                claims: [new Claim(JwtRegisteredClaimNames.Sub, data.UserId.ToString())],
                expires: DateTime.UtcNow.AddMonths(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(refreshToken);
        }

        public GetUserDto DecodeToken(string token)
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            var jwt = jwtHandler.ReadJwtToken(token);

            return new GetUserDto(){UserId = Guid.Parse(jwt.Payload.Sub), Login = jwt.Claims.FirstOrDefault(c => c.Type == "login")?.Value!, Name = jwt.Claims.FirstOrDefault(c => c.Type == "name")?.Value!};
        }

        public string HashToken(string token)
        {
            return BCrypt.Net.BCrypt.HashPassword(token);
        }

        public bool VerifyTokenHashs(string token, string tokenHash)
        {
            return BCrypt.Net.BCrypt.Verify(token, tokenHash);
        }
    }
}