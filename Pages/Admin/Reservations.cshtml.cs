using CarRental.Data;
using CarRental.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class ReservationsModel : PageModel
    {
        private readonly CarRentalContext _context;

        public ReservationsModel(CarRentalContext context) => _context = context;

        public List<ReservationRow> Rows { get; set; } = new();

        public List<ReservationStatus> AllStatuses { get; } =
            Enum.GetValues(typeof(ReservationStatus)).Cast<ReservationStatus>().ToList();

        public class ReservationRow
        {
            public int Id { get; set; }
            public Guid UserId { get; set; }
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
            var data = await (from r in _context.CarReservations.AsNoTracking()
                              join c in _context.Cars.AsNoTracking()
                                on r.CarId equals c.Id
                              orderby r.DateFrom descending
                              select new
                              {
                                  r.Id,
                                  r.UserId,
                                  r.DateFrom,
                                  r.DateTo,
                                  r.Status,
                                  c.Make,
                                  c.Model,
                                  c.Year,
                                  c.DailyPrice
                              }).ToListAsync();

            Rows = data.Select(x =>
            {
                var days = (x.DateTo.Date - x.DateFrom.Date).Days + 1;
                if (days < 1) days = 1;

                return new ReservationRow
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    DateFrom = x.DateFrom.Date,
                    DateTo = x.DateTo.Date,
                    Status = x.Status,
                    CarDisplay = $"{x.Make} {x.Model} ({x.Year})",
                    DailyPrice = x.DailyPrice,
                    Days = days,
                    TotalCost = x.DailyPrice * days
                };
            }).ToList();
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(int id, ReservationStatus status)
        {
            var reservation = await _context.CarReservations.FirstOrDefaultAsync(r => r.Id == id);
            if (reservation == null) return NotFound();

            reservation.Status = status;
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }
    }
}
