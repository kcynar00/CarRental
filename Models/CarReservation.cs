using System;
namespace CarRental.Models;

public enum ReservationStatus
{
    Aktywna = 1,
    Anulowana = 2
}

public class CarReservation
{
    public int Id { get; set; }                // Id rezerwacji (np. PK w bazie)
    public Guid UserId { get; set; }           // np. 526d3616-311d-4ecd-a0dc-3dc469f23bd1
    public int CarId { get; set; }             // Id samochodu
    public DateTime DateFrom { get; set; }     // Data od
    public DateTime DateTo { get; set; }       // Data do
    public ReservationStatus Status { get; set; } = ReservationStatus.Aktywna;
}
