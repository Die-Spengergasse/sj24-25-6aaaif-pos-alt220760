using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using SPG_Fachtheorie.Aufgabe1.Infrastructure;

namespace SPG_Fachtheorie.Aufgabe3.Test
{
    public class TestWebApplicationFactory : WebApplicationFactory<SPG_Fachtheorie.Aufgabe3.Program>
    {
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
                    options.UseInMemoryDatabase("TestDb");
                });

                // Datenbank initialisieren
                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<AppointmentContext>();
                    db.Database.EnsureCreated();
                    db.Seed(); // Testdaten einfügen
                }
            });
        }
    }
}
