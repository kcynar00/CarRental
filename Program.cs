using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CarRental.Data; // <--- Upewnij się, że to pasuje do Twojego namespace'a
using CarRental.Models;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);


var connectionString = builder.Configuration.GetConnectionString("CarRentalContext")
    ?? throw new InvalidOperationException("Connection string 'CarRentalContext' not found.");

builder.Services.AddDbContext<CarRentalContext>(options =>
    options.UseSqlite(connectionString));


builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    {

        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireDigit = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 3;
    })
    .AddEntityFrameworkStores<CarRentalContext>();

builder.Services.AddRazorPages();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

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
            var cars = new List<Car>
            {
                new Car { Make = "Toyota", Model = "Corolla", Year = 2021, DailyPrice = 149.99m },
                new Car { Make = "Toyota", Model = "Yaris", Year = 2020, DailyPrice = 129.99m },
                new Car { Make = "Volkswagen", Model = "Golf", Year = 2019, DailyPrice = 139.99m },
                new Car { Make = "Skoda", Model = "Octavia", Year = 2022, DailyPrice = 169.99m },
                new Car { Make = "Kia", Model = "Ceed", Year = 2021, DailyPrice = 149.00m },
                new Car { Make = "Hyundai", Model = "i30", Year = 2020, DailyPrice = 139.00m },
                new Car { Make = "BMW", Model = "3 Series", Year = 2021, DailyPrice = 299.99m },
                new Car { Make = "Audi", Model = "A4", Year = 2022, DailyPrice = 319.99m }
            };
            await context.Cars.AddRangeAsync(cars);
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
                var debugNewUser = await userManager.FindByEmailAsync(testEmail);
                logger.LogInformation("Created test user {email} with password 'Test123!'", testEmail, debugNewUser);
            }
            else
            {
                logger.LogWarning("Failed to create test user: {errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        // Seed test cars
        if (await context.Cars.AnyAsync() == false)
        {
            var cars = new List<Car>
         {
            new Car { Make = "Toyota", Model = "Corolla", Year = 2021, DailyPrice = 149.99m },
            new Car { Make = "Toyota", Model = "Yaris", Year = 2020, DailyPrice = 129.99m },
            new Car { Make = "Volkswagen", Model = "Golf", Year = 2019, DailyPrice = 139.99m },
            new Car { Make = "Skoda", Model = "Octavia", Year = 2022, DailyPrice = 169.99m },
            new Car { Make = "Kia", Model = "Ceed", Year = 2021, DailyPrice = 149.00m },
            new Car { Make = "Hyundai", Model = "i30", Year = 2020, DailyPrice = 139.00m },
            new Car { Make = "BMW", Model = "3 Series", Year = 2021, DailyPrice = 299.99m },
            new Car { Make = "Audi", Model = "A4", Year = 2022, DailyPrice = 319.99m }
         };
            await context.Cars.AddRangeAsync(cars);
            await context.SaveChangesAsync();
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
