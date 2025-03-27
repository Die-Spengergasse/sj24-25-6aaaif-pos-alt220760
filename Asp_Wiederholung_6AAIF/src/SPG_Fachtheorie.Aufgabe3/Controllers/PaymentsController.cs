using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPG_Fachtheorie.Aufgabe1.Infrastructure;
using SPG_Fachtheorie.Aufgabe1.Model;
using SPG_Fachtheorie.Aufgabe3.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SPG_Fachtheorie.Aufgabe3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly AppointmentContext _db;

        public PaymentsController(AppointmentContext db)
        {
            _db = db;
        }

        // GET /api/payments?cashDesk=number&dateFrom=YYYY-MM-DD
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetPayments([FromQuery] int? number, [FromQuery] DateTime? dateFrom)
        {
            var result = await _db.Payments
                .Include(p => p.CashDesk)
                .Include(p => p.Employee)
                .Include(p => p.PaymentItems)
                .Where(p => number == null || p.CashDesk.Number == number)
                .Where(p => dateFrom == null || p.PaymentDateTime >= dateFrom)
                .Select(p => new PaymentDto(
                    p.Id,
                    p.Employee.FirstName,
                    p.Employee.LastName,
                    p.CashDesk.Number,
                    p.PaymentType.ToString(),
                    p.PaymentItems.Sum(i => i.Price)
                ))
                .ToListAsync();

            return Ok(result);
        }

        // GET /api/payments/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentDetailDto>> GetPayment(int id)
        {
            var data = await _db.Payments
                .Include(p => p.CashDesk)
                .Include(p => p.Employee)
                .Include(p => p.PaymentItems)
                .Where(p => p.Id == id)
                .Select(p => new PaymentDetailDto(
                    p.Id,
                    p.Employee.FirstName,
                    p.Employee.LastName,
                    p.CashDesk.Number,
                    p.PaymentType.ToString(),
                    p.PaymentItems.Select(i => new PaymentItemDto(
                        i.ArticleName,
                        i.Amount,
                        i.Price
                    )).ToList()
                ))
                .FirstOrDefaultAsync();

            if (data is null)
            {
                return NotFound();
            }

            return Ok(data);
        }

        // PUT /api/paymentItems/{id}
        [HttpPut("paymentItems/{id}")]
        public async Task<IActionResult> UpdatePaymentItem(int id, [FromBody] UpdatePaymentItemCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("Invalid payment item ID");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var paymentItem = await _db.PaymentItems.FirstOrDefaultAsync(pi => pi.Id == id);
            if (paymentItem == null)
            {
                return NotFound("Payment Item not found");
            }

            if (command.LastUpdated.HasValue)
            {
                if (paymentItem.LastUpdated != command.LastUpdated)
                {
                    return BadRequest("Payment item has changed");
                }
            }

            var payment = await _db.Payments.FirstOrDefaultAsync(p => p.Id == command.PaymentId);
            if (payment == null)
            {
                return BadRequest("Invalid payment ID");
            }

            paymentItem.ArticleName = command.ArticleName;
            paymentItem.Amount = command.Amount;
            paymentItem.Price = command.Price;
            paymentItem.LastUpdated = DateTime.Now;

            await _db.SaveChangesAsync();

            return NoContent();
        }

        // PATCH /api/payments/{id}
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdatePaymentConfirmed(int id, [FromBody] UpdateConfirmedCommand command)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var payment = await _db.Payments.FirstOrDefaultAsync(p => p.Id == id);
            if (payment == null)
            {
                return NotFound("Payment not found");
            }

            if (payment.Confirmed != null)
            {
                return BadRequest("Payment already confirmed");
            }

            payment.Confirmed = command.Confirmed;
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}
