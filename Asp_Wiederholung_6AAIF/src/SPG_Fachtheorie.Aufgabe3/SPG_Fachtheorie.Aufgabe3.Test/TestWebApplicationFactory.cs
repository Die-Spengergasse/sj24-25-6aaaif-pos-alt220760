using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using SPG_Fachtheorie.Aufgabe1.Infrastructure;
using System;
using System.Net;
using System.Threading.Tasks;

namespace SPG_Fachtheorie.Aufgabe3.Test
{
    public class TestWebApplicationFactory : WebApplicationFactory<Program>
    {
        private Action<AppointmentContext> _dbInitializer;

        public void InitializeDatabase(Action<AppointmentContext> dbInitializer)
        {
            _dbInitializer = dbInitializer;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Finde den DbContext-Service
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppointmentContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // In-Memory-Datenbank für Tests
                services.AddDbContext<AppointmentContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid().ToString());
                });

                // Datenbank initialisieren
                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<AppointmentContext>();
                    db.Database.EnsureCreated();

                    if (_dbInitializer != null)
                    {
                        _dbInitializer(db);
                    }
                    else
                    {
                        // Standard-Testdaten einfügen
                        db.Seed();
                    }
                }
            });
        }

        public async Task<(HttpStatusCode StatusCode, T Content)> GetHttpContent<T>(string url)
        {
            var client = this.CreateClient();
            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = System.Text.Json.JsonSerializer.Deserialize<T>(
                    content,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return (response.StatusCode, result);
            }

            return (response.StatusCode, default);
        }
    }
}
