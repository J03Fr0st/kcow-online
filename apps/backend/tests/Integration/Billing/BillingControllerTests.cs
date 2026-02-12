using Kcow.Application.Auth;
using Kcow.Application.Billing;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Kcow.Integration.Tests.Billing;

public class BillingControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public BillingControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateHttpsClient()
    {
        return _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = CreateHttpsClient();

        var loginRequest = new
        {
            Email = "admin@kcow.local",
            Password = "Admin123!"
        };

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(loginResult);
        Assert.NotNull(loginResult.Token);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginResult.Token);

        return client;
    }

    // ---- Authentication Tests ----

    [Fact]
    public async Task GetBillingSummary_WithoutAuth_Returns401()
    {
        var client = CreateHttpsClient();
        var response = await client.GetAsync("/api/students/1/billing");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetInvoices_WithoutAuth_Returns401()
    {
        var client = CreateHttpsClient();
        var response = await client.GetAsync("/api/students/1/invoices");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetPayments_WithoutAuth_Returns401()
    {
        var client = CreateHttpsClient();
        var response = await client.GetAsync("/api/students/1/payments");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateInvoice_WithoutAuth_Returns401()
    {
        var client = CreateHttpsClient();
        var request = new CreateInvoiceRequest
        {
            InvoiceDate = "2026-02-01",
            Amount = 1000m,
            DueDate = "2026-03-01"
        };
        var response = await client.PostAsJsonAsync("/api/students/1/invoices", request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreatePayment_WithoutAuth_Returns401()
    {
        var client = CreateHttpsClient();
        var request = new CreatePaymentRequest
        {
            PaymentDate = "2026-02-10",
            Amount = 500m,
            PaymentMethod = 0
        };
        var response = await client.PostAsJsonAsync("/api/students/1/payments", request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ---- Billing Summary Tests ----

    [Fact]
    public async Task GetBillingSummary_WithNonExistingStudent_Returns404()
    {
        var client = await CreateAuthenticatedClientAsync();
        var response = await client.GetAsync("/api/students/99999/billing");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ---- Invoice Endpoint Tests ----

    [Fact]
    public async Task GetInvoices_WithNonExistingStudent_Returns404()
    {
        var client = await CreateAuthenticatedClientAsync();
        var response = await client.GetAsync("/api/students/99999/invoices");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateInvoice_WithInvalidData_Returns400()
    {
        var client = await CreateAuthenticatedClientAsync();

        // Missing required fields / invalid amount
        var request = new { InvoiceDate = "not-a-date", Amount = -1, DueDate = "" };
        var response = await client.PostAsJsonAsync("/api/students/1/invoices", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ---- Payment Endpoint Tests ----

    [Fact]
    public async Task GetPayments_WithNonExistingStudent_Returns404()
    {
        var client = await CreateAuthenticatedClientAsync();
        var response = await client.GetAsync("/api/students/99999/payments");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreatePayment_WithInvalidData_Returns400()
    {
        var client = await CreateAuthenticatedClientAsync();

        var request = new { PaymentDate = "not-a-date", Amount = -1, PaymentMethod = 99 };
        var response = await client.PostAsJsonAsync("/api/students/1/payments", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreatePayment_WithInvalidPaymentMethod_Returns400()
    {
        var client = await CreateAuthenticatedClientAsync();

        var request = new CreatePaymentRequest
        {
            PaymentDate = "2026-02-10",
            Amount = 500m,
            PaymentMethod = 5 // Invalid: must be 0-3
        };
        var response = await client.PostAsJsonAsync("/api/students/1/payments", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
