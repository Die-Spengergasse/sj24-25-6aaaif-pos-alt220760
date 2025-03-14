using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SPG_Fachtheorie.Aufgabe1.Infrastructure;
using SPG_Fachtheorie.Aufgabe3.Dtos;

namespace SPG_Fachtheorie.Aufgabe3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly AppointmentContext _db; // DB-Context, damit wir auf die Daten zugreifen können

        public PaymentsController(AppointmentContext db)
        {
            _db = db; // Context speichern
        }

        [HttpGet] // GET-Request für alle Payments
        public ActionResult<PaymentDto> GetPayments([FromQuery] int? number, [FromQuery] DateTime? dateFrom)
        {
            return Ok(_db.Payments
                .Where(p => number == null || p.CashDesk.Number == number) // Falls number angegeben ist, filtern
                .Where(p => dateFrom == null || p.PaymentDateTime >= dateFrom) // Falls dateFrom angegeben, filtern
                .Select(
                        p => new PaymentDto(
                                p.Id,
                                p.Employee.FirstName,
                                p.Employee.LastName,
                                p.CashDesk.Number,
                                p.PaymentType.ToString(),
                                p.PaymentItems.Where(i => i.Payment == p).Sum(i => i.Price) // Summe der Preise berechnen
                            )
                   ));
        }

        [HttpGet("{id}")] // GET-Request für eine bestimmte Zahlung
        public ActionResult<PaymentDetailDto> GetPayment(int id)
        {
            var data = _db.Payments
                .Where(p => p.Id == id) // Filtern nach ID
                .Select(p =>
                    new PaymentDetailDto(
                        p.Id,
                        p.Employee.FirstName,
                        p.Employee.LastName,
                        p.CashDesk.Number,
                        p.PaymentType.ToString(),
                        p.PaymentItems.Select(i =>
                            new PaymentItemDto(
                                i.ArticleName, // Name des Artikels
                                i.Amount,      // Anzahl der gekauften Artikel
                                i.Price        // Preis pro Artikel
                            )
                        ).ToList()
                    )
                ).FirstOrDefault(); // Falls nichts gefunden wird -> null

            if (data is null) return NotFound(); // Falls nix gefunden wurde, Fehler zurückgeben
            return Ok(data); // Ansonsten Daten zurückgeben
        }
    }
}