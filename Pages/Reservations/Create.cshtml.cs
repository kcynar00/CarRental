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

        public CreateModel(CarRentalContext context) => _context = context;

        public List<SelectListItem> CarOptions { get; set; } = new();

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required(ErrorMessage = "Wybierz samochód.")]
            public int? CarId { get; set; }

            [Required, DataType(DataType.Date)]
            public DateTime DateFrom { get; set; } = DateTime.Today;

            [Required, DataType(DataType.Date)]
            public DateTime DateTo { get; set; } = DateTime.Today.AddDays(1);
        }

        public async Task OnGetAsync(int? carId)
        {
            await LoadCarsAsync();
            if (carId.HasValue) Input.CarId = carId.Value;
        }

        // <-- TO: endpoint dla JS: /Reservations/Create?handler=ReservedRanges&carId=5
        public async Task<IActionResult> OnGetReservedRangesAsync(int carId)
        {
            var ranges = await _context.CarReservations
                .AsNoTracking()
                .Where(r => r.CarId == carId && r.Status == ReservationStatus.Aktywna)
                .Select(r => new
                {
                    from = r.DateFrom.Date.ToString("yyyy-MM-dd"),
                    to = r.DateTo.Date.ToString("yyyy-MM-dd")
                })
                .ToListAsync();

            return new JsonResult(ranges);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadCarsAsync();

            if (!ModelState.IsValid)
                return Page();

            if (Input.DateTo.Date < Input.DateFrom.Date)
            {
                ModelState.AddModelError(nameof(Input.DateTo), "Data do nie może być wcześniejsza niż data od.");
                return Page();
            }

            // Serwerowa walidacja kolizji (MUSI być, nawet jak UI blokuje)
            var from = Input.DateFrom.Date;
            var to = Input.DateTo.Date;

            bool overlaps = await _context.CarReservations.AnyAsync(r =>
                r.CarId == Input.CarId!.Value &&
                r.Status == ReservationStatus.Aktywna &&
                r.DateFrom.Date <= to &&
                r.DateTo.Date >= from);

            if (overlaps)
            {
                ModelState.AddModelError(string.Empty, "Wybrany termin jest niedostępny dla tego samochodu.");
                return Page();
            }

            // UWAGA: tu zakładam, że NameIdentifier jest GUID-em (tak masz w encji).
            // Jeśli masz domyślne Identity z string ID, daj znać – zmienimy UserId w encji albo konfigurację Identity.
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdStr) || !Guid.TryParse(userIdStr, out var userGuid))
            {
                ModelState.AddModelError(string.Empty, "Nie udało się odczytać ID użytkownika jako GUID.");
                return Page();
            }

            _context.CarReservations.Add(new CarReservation
            {
                UserId = userGuid,
                CarId = Input.CarId!.Value,
                DateFrom = from,
                DateTo = to,
                Status = ReservationStatus.Aktywna
            });

            await _context.SaveChangesAsync();
            return RedirectToPage("/Cars");
        }

        private async Task LoadCarsAsync()
        {
            CarOptions = await _context.Cars
                .AsNoTracking()
                .OrderBy(c => c.Make).ThenBy(c => c.Model)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.Make} {c.Model} ({c.Year}) - {c.DailyPrice:0.00} / dzień"
                })
                .ToListAsync();
        }
    }
}
