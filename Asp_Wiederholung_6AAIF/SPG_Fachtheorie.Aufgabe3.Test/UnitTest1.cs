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
    public class PaymentsControllerTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly TestWebApplicationFactory _factory;

        public PaymentsControllerTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Theory]
        [InlineData(1, null, 2)]  // cashDesk=1
        [InlineData(null, "2024-05-13", 3)]  // dateFrom
        [InlineData(1, "2024-05-13", 1)]  // both parameters
        public async Task GetPayments_WithFilters_ReturnsFilteredResults(
            int? cashDesk,
            string? dateFrom,
            int expectedCount)
        {
            // Arrange
            var client = _factory.CreateClient();
            var query = "";
            if (cashDesk.HasValue)
                query += $"cashDesk={cashDesk}";
            if (dateFrom != null)
            {
                if (query != "") query += "&";
                query += $"dateFrom={dateFrom}";
            }

            // Act
            var response = await client.GetAsync($"/api/payments?{query}");

            // Assert
            response.EnsureSuccessStatusCode();
            var payments = await DeserializeResponse<List<Payment>>(response);
            Assert.Equal(expectedCount, payments.Count);

            if (cashDesk.HasValue)
            {
                foreach (var p in payments)
                {
                    Assert.Equal(cashDesk.Value, p.CashDesk.Number);
                }
            }
            if (dateFrom != null)
            {
                var fromDate = DateTime.Parse(dateFrom);
                foreach (var p in payments)
                {
                    Assert.True(p.PaymentDateTime >= fromDate);
                }
            }
        }

        [Theory]
        [InlineData(1, HttpStatusCode.OK)]  // Existing ID
        [InlineData(999, HttpStatusCode.NotFound)]  // Non-existing ID
        public async Task GetPaymentById_ReturnsCorrectResponse(int id, HttpStatusCode expectedStatus)
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync($"/api/payments/{id}");

            // Assert
            Assert.Equal(expectedStatus, response.StatusCode);
        }

        [Theory]
        [InlineData(1, HttpStatusCode.OK)]  // Unconfirmed payment
        [InlineData(2, HttpStatusCode.BadRequest)]  // Already confirmed
        [InlineData(999, HttpStatusCode.NotFound)]  // Non-existing
        public async Task PatchPayment_ReturnsCorrectResponse(int id, HttpStatusCode expectedStatus)
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.PatchAsync($"/api/payments/{id}", null);

            // Assert
            Assert.Equal(expectedStatus, response.StatusCode);
        }

        [Theory]
        [InlineData(1, HttpStatusCode.NoContent)]  // Existing payment
        [InlineData(999, HttpStatusCode.NotFound)]  // Non-existing payment
        public async Task DeletePayment_ReturnsCorrectResponse(int id, HttpStatusCode expectedStatus)
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.DeleteAsync($"/api/payments/{id}");

            // Assert
            Assert.Equal(expectedStatus, response.StatusCode);
        }

        private async Task<T> DeserializeResponse<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }
    }
}
