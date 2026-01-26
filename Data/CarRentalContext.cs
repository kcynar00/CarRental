using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Data;

using CarRental.Models;

public class CarRentalContext : IdentityDbContext<IdentityUser>
{
    public CarRentalContext(DbContextOptions<CarRentalContext> options)
        : base(options)
    {
    }

    // DbSet for test Car entity
    public DbSet<Car> Cars { get; set; } = null!;
    public DbSet<CarReservation> CarReservations { get; set; } = null!;
}
