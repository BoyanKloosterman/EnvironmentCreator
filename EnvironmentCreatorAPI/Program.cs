using EnvironmentCreatorAPI.Data;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Avans.Identity.Dapper;
using Microsoft.AspNetCore.Identity.Data;
using EnvironmentCreatorAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Keep your existing DbContext configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add logging
builder.Services.AddLogging();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// or use the logger once it's configured
// Add Identity Framework with Dapper (correct configuration order)
builder.Services
    .AddAuthorization()
    .AddIdentityApiEndpoints<IdentityUser>()
    .AddDapperStores(options =>
        options.ConnectionString = builder.Configuration.GetConnectionString("DefaultConnection"));

// Add HttpContextAccessor for accessing the current user
builder.Services.AddHttpContextAccessor();

// Register the AuthenticationService
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

builder.Services
    .AddOptions<BearerTokenOptions>()
    .Bind(builder.Configuration.GetSection("BearerToken"))
    .Configure(options =>
    {
        options.BearerTokenExpiration = TimeSpan.FromHours(1);
        options.RefreshTokenExpiration = TimeSpan.FromDays(7);
    });

// Keep your existing controllers and Swagger configuration
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.None;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
app.MapGet("/", () => $"API is up");
app.UseHttpsRedirection();
app.UseSwagger();
app.UseSwaggerUI();
// Enable authorization and authentication
app.UseAuthentication();
app.UseAuthorization();

// Map Identity API endpoints with correct syntax for ASP.NET Core 8
app.MapGroup("account")
   .MapIdentityApi<IdentityUser>();

app.MapPost("/account/logout", async (SignInManager<IdentityUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.Ok();
});

app.MapControllers().RequireAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();