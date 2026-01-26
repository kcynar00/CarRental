using CarRental.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Pages
{
    public class CarsModel : PageModel
    {
        private readonly CarRental.Data.CarRentalContext _context;

        public CarsModel(CarRental.Data.CarRentalContext context)
        {
            _context = context;
        }

        public IList<Car> Cars { get; set; } = new List<Car>();

        public async Task OnGetAsync()
        {
            Cars = await _context.Cars
                .AsNoTracking()
                .OrderBy(c => c.Make)
                .ThenBy(c => c.Model)
                .ToListAsync();
        }
    }
}
