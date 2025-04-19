using _3CXCallogScrapper.Models;
using _3CXCallogScrapper.Services;
using _3CXCallogScrapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;


var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHttpClient();

// Add configuration
builder.Services.Configure<ThreeCXApiSettings>(
    builder.Configuration.GetSection("3CXApiSettings"));

// Add DB context
builder.Services.AddDbContext<CallLogDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL")));

// Add services
builder.Services.AddSingleton<AuthenticationService>();
builder.Services.AddScoped<DatabaseService>();
builder.Services.AddHostedService<CallLogScraperService>();

// Build and run the host
var host = builder.Build();
await host.RunAsync();
