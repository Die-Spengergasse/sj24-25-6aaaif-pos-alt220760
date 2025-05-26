using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using System.Net;
using SPG_Fachtheorie.Aufgabe1.Model;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace SPG_Fachtheorie.Aufgabe3.Test
{
    public class PaymentsControllerTests
    {
        [Theory]
        [InlineData(1, "2025-03-15", 1)]  // Filter nach Kassa 1 mit Datum >= 2025-03-15 --> 1 Payment
        [InlineData(1, null, 2)]          // Filter nach Kassa 1                         --> 2 Payments
        [InlineData(null, "2025-03-15", 2)]  // Filter nach Datum >= 2025-03-15          --> 2 Payments
        [InlineData(null, null, 4)]       // Ohne Filter                                 --> 4 Payments
        public async Task GetAllPaymentsTest(int? cashDeskNumber, string? dateFromStr, int expectedCount)
        {
            DateTime? dateFrom = dateFromStr is null ? null : DateTime.Parse(dateFromStr);

            // ARRANGE
            var factory = new TestWebApplicationFactory();
            factory.InitializeDatabase((db) =>
            {
                var cashDesk1 = new CashDesk(1);
                var cashDesk2 = new CashDesk(2);

                var casher = new Cashier(
                    1, "FN", "LN", new DateOnly(2000, 1, 1), 3000M, null, "Feinkost");

                var payment1 = new Payment(
                    cashDesk1, new DateTime(2025, 3, 1, 12, 0, 0), casher, PaymentType.Cash)
                { Confirmed = new DateTime(2025, 3, 2, 0, 0, 0) };

                var payment2 = new Payment(
                    cashDesk1, new DateTime(2025, 4, 1, 12, 0, 0), casher, PaymentType.Cash)
                { Confirmed = new DateTime(2025, 4, 2, 0, 0, 0) };

                var payment3 = new Payment(
                    cashDesk2, new DateTime(2025, 3, 1, 12, 0, 0), casher, PaymentType.Cash)
                { Confirmed = new DateTime(2025, 3, 2, 0, 0, 0) };

                var payment4 = new Payment(
                    cashDesk2, new DateTime(2025, 4, 1, 12, 0, 0), casher, PaymentType.Cash)
                { Confirmed = new DateTime(2025, 4, 2, 0, 0, 0) };

                db.AddRange(payment1, payment2, payment3, payment4);
                db.SaveChanges();
            });

            // ACT
            string url = "/api/payments";
            if (cashDeskNumber.HasValue || dateFrom.HasValue)
            {
                url += "?";
                if (cashDeskNumber.HasValue)
                {
                    url += $"cashDesk={cashDeskNumber}";
                }

                if (dateFrom.HasValue)
                {
                    if (cashDeskNumber.HasValue) url += "&";
                    url += $"dateFrom={dateFromStr}";
                }
            }

            var (statusCode, content) = await factory.GetHttpContent<List<Payment>>(url);

            // ASSERT
            Assert.Equal(HttpStatusCode.OK, statusCode);
            Assert.NotNull(content);
            Assert.Equal(expectedCount, content.Count);

            if (cashDeskNumber.HasValue)
            {
                Assert.All(content, p => Assert.Equal(cashDeskNumber.Value, p.CashDesk.Number));
            }

            if (dateFrom.HasValue)
            {
                Assert.All(content, p => Assert.True(p.PaymentDateTime >= dateFrom.Value));
            }
        }

        [Theory]
        [InlineData(1, HttpStatusCode.OK)]           // Existierender Payment
        [InlineData(999, HttpStatusCode.NotFound)]   // Nicht existierender Payment
        public async Task GetPaymentByIdTest(int paymentId, HttpStatusCode expectedStatus)
        {
            // ARRANGE
            var factory = new TestWebApplicationFactory();

            // ACT
            var client = factory.CreateClient();
            var response = await client.GetAsync($"/api/payments/{paymentId}");

            // ASSERT
            Assert.Equal(expectedStatus, response.StatusCode);
        }

        [Theory]
        [InlineData(1, HttpStatusCode.OK)]           // Unbestätigter Payment
        [InlineData(2, HttpStatusCode.BadRequest)]   // Bereits bestätigter Payment
        [InlineData(999, HttpStatusCode.NotFound)]   // Nicht existierender Payment
        public async Task PatchPaymentTest(int paymentId, HttpStatusCode expectedStatus)
        {
            // ARRANGE
            var factory = new TestWebApplicationFactory();

            // ACT
            var client = factory.CreateClient();
            var response = await client.PatchAsync($"/api/payments/{paymentId}", null);

            // ASSERT
            Assert.Equal(expectedStatus, response.StatusCode);
        }

        [Theory]
        [InlineData(1, HttpStatusCode.NoContent)]    // Existierender Payment
        [InlineData(999, HttpStatusCode.NotFound)]   // Nicht existierender Payment
        public async Task DeletePaymentTest(int paymentId, HttpStatusCode expectedStatus)
        {
            // ARRANGE
            var factory = new TestWebApplicationFactory();

            // ACT
            var client = factory.CreateClient();
            var response = await client.DeleteAsync($"/api/payments/{paymentId}");

            // ASSERT
            Assert.Equal(expectedStatus, response.StatusCode);
        }
    }
}
