using CarRental.Data;
using CarRental.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CarRental.Pages.Reservations
{
    [Authorize]
    public class MyModel : PageModel
    {
        private readonly CarRentalContext _context;

        public MyModel(CarRentalContext context) => _context = context;

        public string? ErrorMessage { get; set; }

        public List<ReservationVm> Reservations { get; set; } = new();

        public class ReservationVm
        {
            public DateTime DateFrom { get; set; }
            public DateTime DateTo { get; set; }
            public ReservationStatus Status { get; set; }

            public string CarDisplay { get; set; } = "";
            public decimal DailyPrice { get; set; }

            public int Days { get; set; }
            public decimal TotalCost { get; set; }
        }

        public async Task OnGetAsync()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Masz UserId w rezerwacji jako Guid -> tu próbujemy sparsować
            if (string.IsNullOrWhiteSpace(userIdStr) || !Guid.TryParse(userIdStr, out var userGuid))
            {
                ErrorMessage = "Nie udało się odczytać ID użytkownika jako GUID (sprawdź konfigurację Identity).";
                return;
            }

            // Pobieramy rezerwacje + dane auta
            var rows = await (from r in _context.CarReservations.AsNoTracking()
                              join c in _context.Cars.AsNoTracking()
                                on r.CarId equals c.Id
                              where r.UserId == userGuid
                              orderby r.DateFrom descending
                              select new
                              {
                                  r.DateFrom,
                                  r.DateTo,
                                  r.Status,
                                  c.Make,
                                  c.Model,
                                  c.Year,
                                  c.DailyPrice
                              })
                              .ToListAsync();

            Reservations = rows.Select(x =>
            {
                // Zakładamy zakres INCLUSIVE (od i do wliczane)
                var days = (x.DateTo.Date - x.DateFrom.Date).Days + 1;
                if (days < 1) days = 1;

                var total = x.DailyPrice * days;

                return new ReservationVm
                {
                    DateFrom = x.DateFrom.Date,
                    DateTo = x.DateTo.Date,
                    Status = x.Status,
                    CarDisplay = $"{x.Make} {x.Model} ({x.Year})",
                    DailyPrice = x.DailyPrice,
                    Days = days,
                    TotalCost = total
                };
            }).ToList();
        }
    }
}
