using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CarRental.Data; // <--- Upewnij się, że to pasuje do Twojego namespace'a
using CarRental.Models;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// 1. POBRANIE CONNECTION STRINGA (Połączenie do bazy)
// Sprawdź w appsettings.json, czy klucz to "CarRentalContext" czy "DefaultConnection"
var connectionString = builder.Configuration.GetConnectionString("CarRentalContext") 
    ?? throw new InvalidOperationException("Connection string 'CarRentalContext' not found.");

// 2. REJESTRACJA BAZY DANYCH
builder.Services.AddDbContext<CarRentalContext>(options =>
    options.UseSqlite(connectionString));

// 3. REJESTRACJA IDENTITY (To naprawia Twój błąd!)
builder.Services.AddDefaultIdentity<IdentityUser>(options => 
    {
        // Opcjonalne: Ułatwienia logowania na czas testów
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireDigit = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 3; 
    })
    .AddEntityFrameworkStores<CarRentalContext>();

// Dodanie Razor Pages
builder.Services.AddRazorPages();

var app = builder.Build();

// Konfiguracja potoku HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Warto dodać dla pewności ładowania stylów CSS

app.UseRouting();

// 4. WAŻNE: KOLEJNOŚĆ (Authentication musi być przed Authorization)
app.UseAuthentication(); 
app.UseAuthorization();

// Mapowanie stron Razor
// Seed test data (cars + test user)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<CarRentalContext>();
        // Create DB file and schema for SQLite
        context.Database.EnsureCreated();

        // Seed Cars if empty
        if (!context.Cars.Any())
        {
            context.Cars.Add(new Car { Make = "Toyota", Model = "Corolla", Year = 2020, DailyPrice = 99m });
            context.Cars.Add(new Car { Make = "Ford", Model = "Focus", Year = 2019, DailyPrice = 79m });
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded sample cars.");
        }

        // Seed test user
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var testEmail = "test@local.test";
        var testUser = await userManager.FindByEmailAsync(testEmail);
        if (testUser == null)
        {
            testUser = new IdentityUser { UserName = testEmail, Email = testEmail, EmailConfirmed = true };
            var result = await userManager.CreateAsync(testUser, "Test123!");
            if (result.Succeeded)
            {
                logger.LogInformation("Created test user {email} with password 'Test123!'", testEmail);
            }
            else
            {
                logger.LogWarning("Failed to create test user: {errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
    catch (Exception ex)
    {
        var logger2 = services.GetRequiredService<ILogger<Program>>();
        logger2.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.MapRazorPages();

app.Run();