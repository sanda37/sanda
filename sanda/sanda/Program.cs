using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using sanda.Data;
using sanda.Repositories;
using sanda.Services;
using sanda.Settings;

var builder = WebApplication.CreateBuilder(args);

// ============= Configuration Settings =============
try
{
    builder.Configuration
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
        .AddEnvironmentVariables();
}
catch (Exception ex)
{
    Console.WriteLine($"Error loading configuration: {ex.Message}");
    throw;
}

// ============= Services Configuration =============
// Mail Settings
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));

// Mailing Service
builder.Services.AddTransient<IMailingService, MailingService>();

// Database Configuration
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")),
    ServiceLifetime.Transient);

// Repository Services
builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
builder.Services.AddScoped<IVolunteerService, VolunteerService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrdersRepo>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<IVolunteerBalanceService, VolunteerBalanceService>();

// Add IWebHostEnvironment
builder.Services.AddSingleton<IWebHostEnvironment>(builder.Environment);

// ============= Controllers & API Configuration =============
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

// ============= Swagger/OpenAPI =============
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ============= Middleware Pipeline =============
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();