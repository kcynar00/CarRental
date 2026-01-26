using CarRental.Data;
using CarRental.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace CarRental.Pages.Reservations
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly CarRentalContext _context;

        public CreateModel(CarRentalContext context)
        {
            _context = context;
        }

        public List<SelectListItem> CarOptions { get; set; } = new();

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Wybierz samochód.")]
            public int? CarId { get; set; }

            [Required(ErrorMessage = "Podaj datę od.")]
            [DataType(DataType.Date)]
            public DateTime DateFrom { get; set; } = DateTime.Today;

            [Required(ErrorMessage = "Podaj datę do.")]
            [DataType(DataType.Date)]
            public DateTime DateTo { get; set; } = DateTime.Today.AddDays(1);
        }

        public async Task OnGetAsync(int? carId)
        {
            await LoadCarsAsync();

            // Jeśli przyszliśmy z listy aut (?carId=...), ustawiamy domyślny wybór
            if (carId.HasValue)
                Input.CarId = carId.Value;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadCarsAsync();

            if (!ModelState.IsValid)
                return Page();

            if (Input.DateTo < Input.DateFrom)
            {
                ModelState.AddModelError(nameof(Input.DateTo), "Data do nie może być wcześniejsza niż data od.");
                return Page();
            }

            // UserId jako Guid z Identity
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdStr) || !Guid.TryParse(userIdStr, out var userGuid))
            {
                ErrorMessage = "Nie udało się odczytać ID użytkownika jako GUID.";
                return Page();
            }

            var reservation = new CarReservation
            {
                UserId = userGuid,
                CarId = Input.CarId!.Value,
                DateFrom = Input.DateFrom,
                DateTo = Input.DateTo,
                Status = ReservationStatus.Aktywna
            };

            _context.CarReservations.Add(reservation);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Cars");
        }

        private async Task LoadCarsAsync()
        {
            CarOptions = await _context.Cars
                .AsNoTracking()
                .OrderBy(c => c.Make)
                .ThenBy(c => c.Model)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.Make} {c.Model} ({c.Year}) - {c.DailyPrice:0.00} / dzień"
                })
                .ToListAsync();
        }
    }
}
