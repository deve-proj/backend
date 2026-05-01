using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Prometheus;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using DeveSecurity;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();
builder.Services.AddControllers();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    
    try
    {
        options.UseNpgsql(connectionString);
    }

    catch(Exception e)
    {
        Console.WriteLine("Error!" + e.Message);
    }
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173", "http://localhost:5174")
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
});

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
    })

    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["secretKey"];

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidateAudience = false,
            ValidIssuer = jwtSettings["Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!))
        };
    })

    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.Cookie.Name = "GoogleAuth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.None;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
        options.SlidingExpiration = true;
    })

    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
        options.Scope.Add("profile");
        options.Scope.Add("email");
        options.SaveTokens = true;

        options.SignInScheme =  CookieAuthenticationDefaults.AuthenticationScheme;

        options.Events = new OAuthEvents
        {
            OnCreatingTicket = async context =>
            {
                var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);

                var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted);

                response.EnsureSuccessStatusCode();
                
                var jsonString = await response.Content.ReadAsStringAsync();

                var user = JsonDocument.Parse(jsonString).RootElement;

                var email = user.GetProperty("email").GetString();
                var name = user.GetProperty("name").GetString();
                
                context.Identity!.AddClaim(new Claim("email", email!));
                context.Identity.AddClaim(new Claim("name", name!));
                context.Identity.AddClaim(new Claim(ClaimTypes.Email, email!));
                context.Identity.AddClaim(new Claim(ClaimTypes.Name, name!));

                context.RunClaimActions(user);
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddScoped<IUserRepo, UserRepo>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IDeveMinioClient, DeveMinioClient>();
builder.Services.AddScoped<IAuth, Auth>();
builder.Services.AddEndpointsApiExplorer();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5263, o => o.Protocols = HttpProtocols.Http1);
    options.ListenLocalhost(5264, o => o.Protocols = HttpProtocols.Http2);
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

app.MapGrpcReflectionService();
app.MapControllers();
app.MapGrpcService<UserGrpcServcice>();
app.UseCors("AllowReactApp");
app.MapMetrics();
app.UseAuthentication();
app.UseAuthorization();
app.Run();