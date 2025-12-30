using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CarRental.Data; // <--- Upewnij się, że to pasuje do Twojego namespace'a

var builder = WebApplication.CreateBuilder(args);

// 1. POBRANIE CONNECTION STRINGA (Połączenie do bazy)
// Sprawdź w appsettings.json, czy klucz to "CarRentalContext" czy "DefaultConnection"
var connectionString = builder.Configuration.GetConnectionString("CarRentalContext") 
    ?? throw new InvalidOperationException("Connection string 'CarRentalContext' not found.");

// 2. REJESTRACJA BAZY DANYCH
builder.Services.AddDbContext<CarRentalContext>(options =>
    options.UseSqlServer(connectionString));

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

// Mapowanie
app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();