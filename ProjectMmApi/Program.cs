using Microsoft.EntityFrameworkCore;
using ProjectMmApi.Data;
using ProjectMmApi.Services;
using ProjectMmApi.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ProjectMmApi.Controllers.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add user secrets
builder.Configuration.AddUserSecrets<Program>();

// Retrieve database connection string
string dbConnectionString = builder.Configuration["ConnectionStrings:Default"] ??
    throw new ArgumentNullException(nameof(args), "Default connection string not found in secrets");

// Retrieve JWT config
string jwtKey = builder.Configuration["JwtConfig:Key"]
    ?? throw new ArgumentNullException(nameof(args), "JWT key not found in secrets");
string jwtIssuer = builder.Configuration["JwtConfig:Issuer"]
    ?? throw new ArgumentNullException(nameof(args), "JWT issuer not found in secrets");
string jwtAudience= builder.Configuration["JwtConfig:Audience"]
    ?? throw new ArgumentNullException(nameof(args), "JWT audience not found in secrets");

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Connect to database
builder.Services.AddDbContext<ApplicationDbContext>(options => 
    options.UseMySQL(dbConnectionString));

// Configure JWT authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true
    };
});
builder.Services.AddAuthorization();

// Register custom services
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<AuthMiddleware>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors(builder =>
{
    builder.AllowAnyOrigin();
    builder.AllowAnyHeader();
    builder.AllowAnyMethod();
});

app.UseMiddleware<AuthMiddleware>();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
