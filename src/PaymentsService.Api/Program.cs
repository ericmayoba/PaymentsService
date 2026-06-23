using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PaymentsService.Application.Abstractions;
using PaymentsService.Application.Transfers;
using PaymentsService.Application.Wallets;
using PaymentsService.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

var jwtKey = builder.Configuration["Jwt:Key"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;

builder.Services.AddDbContext<PaymentsDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Payments")));

builder.Services.AddScoped<IWalletRepository, WalletRepository>();
builder.Services.AddScoped<IMovementRepository, MovementRepository>();
builder.Services.AddScoped<WalletService>();
builder.Services.AddScoped<TransferService>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapOpenApi();
app.UseAuthentication();
app.UseAuthorization();

// Dev convenience endpoint to obtain a bearer token.
app.MapPost("/v1/auth/token", (TokenRequest req) =>
{
    var creds = new SigningCredentials(
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: jwtIssuer,
        claims: [new Claim(ClaimTypes.Name, req.Username)],
        expires: DateTime.UtcNow.AddHours(1),
        signingCredentials: creds);

    return Results.Ok(new { access_token = new JwtSecurityTokenHandler().WriteToken(token) });
});

app.MapPost("/v1/wallets", async (CreateWalletRequest req, WalletService service) =>
{
    var wallet = await service.CreateAsync(req.DocumentId, req.Name, req.InitialBalance);
    return Results.Created($"/v1/wallets/{wallet.Id}", wallet);
}).RequireAuthorization();

app.MapGet("/v1/wallets/{id:int}", async (int id, WalletService service) =>
{
    var wallet = await service.GetAsync(id);
    return wallet is null ? Results.NotFound() : Results.Ok(wallet);
}).RequireAuthorization();

app.MapPost("/v1/transfers", async (TransferRequest req, TransferService service) =>
{
    var result = await service.TransferAsync(req.FromWalletId, req.ToWalletId, req.Amount);
    return result.Success ? Results.Ok() : Results.BadRequest(new { error = result.Error });
}).RequireAuthorization();

// Public endpoint: movement history does not require authentication.
app.MapGet("/v1/wallets/{id:int}/movements", async (int id, IMovementRepository movements) =>
{
    var list = await movements.GetByWalletIdAsync(id);
    return Results.Ok(list);
});

app.Run();

public record TokenRequest(string Username);
public record CreateWalletRequest(string DocumentId, string Name, double InitialBalance);
public record TransferRequest(int FromWalletId, int ToWalletId, double Amount);

public partial class Program;
