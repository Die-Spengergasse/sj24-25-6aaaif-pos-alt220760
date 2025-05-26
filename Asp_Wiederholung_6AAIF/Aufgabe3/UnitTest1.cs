using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using SPG_Fachtheorie.Aufgabe1.Model;
using System.Text.Json;
using Xunit;

namespace SPG_Fachtheorie.Aufgabe3.Test
{
    [Fact]
    public async Task GetPaymentById_ValidId_ReturnsPayment()
    {
        // Arrange
        var client = _factory.CreateClient();
        var payment = await CreateTestPayment(client);

        // Act
        var response = await client.GetAsync($"/api/payments/{payment.Id}");

        // Assert
        response.EnsureSuccessStatusCode(); // Status 200
        var returnedPayment = await DeserializeResponse<Payment>(response);
        Assert.NotNull(returnedPayment);
        Assert.Equal(payment.Id, returnedPayment.Id);
    }

    [Fact]
    public async Task GetPaymentById_InvalidId_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/payments/99999");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData(true, System.Net.HttpStatusCode.OK)]  // Payment exists and not confirmed
    [InlineData(false, System.Net.HttpStatusCode.NotFound)]  // Payment doesn't exist
    public async Task PatchPayment_ConfirmPayment_ReturnsCorrectStatusCode(bool paymentExists, System.Net.HttpStatusCode expectedStatus)
    {
        // Arrange
        var client = _factory.CreateClient();
        var paymentId = 0;

        if (paymentExists)
        {
            var payment = await CreateTestPayment(client);
            paymentId = payment.Id;
        }
        else
        {
            paymentId = 99999;
        }

        // Act
        var response = await client.PatchAsync($"/api/payments/{paymentId}", null);

        // Assert
        Assert.Equal(expectedStatus, response.StatusCode);
    }

    [Fact]
    public async Task PatchPayment_AlreadyConfirmed_ReturnsBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var payment = await CreateTestPayment(client);

        // Confirm first time
        await client.PatchAsync($"/api/payments/{payment.Id}", null);

        // Act - Try to confirm again
        var response = await client.PatchAsync($"/api/payments/{payment.Id}", null);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData(true, System.Net.HttpStatusCode.NoContent)]  // Payment exists
    [InlineData(false, System.Net.HttpStatusCode.NotFound)]  // Payment doesn't exist
    public async Task DeletePayment_ReturnsCorrectStatusCode(bool paymentExists, System.Net.HttpStatusCode expectedStatus)
    {
        // Arrange
        var client = _factory.CreateClient();
        var paymentId = 0;

        if (paymentExists)
        {
            var payment = await CreateTestPayment(client);
            paymentId = payment.Id;
        }
        else
        {
            paymentId = 99999;
        }

        // Act
        var response = await client.DeleteAsync($"/api/payments/{paymentId}");

        // Assert
        Assert.Equal(expectedStatus, response.StatusCode);
    }

    // Helper method to create a test payment
    private async Task<Payment> CreateTestPayment(HttpClient client)
    {
        var response = await client.PostAsync("/api/payments",
            new StringContent(JsonSerializer.Serialize(new
            {
                CashDeskNumber = 1000,
                PaymentType = "Cash",
                EmployeeRegistrationNumber = 1001
            }), System.Text.Encoding.UTF8, "application/json"));

        response.EnsureSuccessStatusCode();
        return await DeserializeResponse<Payment>(response);
    }

    // Helper method to deserialize response
    private async Task<T> DeserializeResponse<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

}
