using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OBS.Booking.Client.Api;
using OBS.Calendar.Client.Api;
using OBS.LIB.Logging.Extensions;
using OBS.Stamm.Client.Api;
using OBS_Booking_App.Services;
using OBS_Booking_App.Services.API;
using OBS_Booking_App.Services.Configuration;
using OBS_Booking_App.Stores;
using Serilog;
using System;
using System.IO;

namespace OBS_Booking_App
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Booking Simulator started\n");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal("\nThe Application failed to start correctly\n", ex);
                Console.WriteLine("\nThe Application failed to start correctly\n" + ex);
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .AddObsLogging()    // OBS-spezifisches Logging
                .ConfigureServices((hostContext, services) =>
                {
                    // Konfiguration aus appsettings.json laden.
                    // optional: besagt ob diese Datei zwingend ist
                    // reloadOnChange: Datei wird automatisch neu eingelesen, wenn sie zur Laufzeit verändert wird
                    var configBuilder = new ConfigurationBuilder()
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

                    // Wenn appsettings.OBS.Configuration.json vorhanden => OBS-spezifische Konfiguration laden
                    var obsConfigPath = "appsettings.OBS.Configuration.json";
                    var useObsApi = File.Exists(obsConfigPath);
                    if (useObsApi)
                    {
                        configBuilder.AddJsonFile(obsConfigPath, optional: true, reloadOnChange: true);
                    }

                    // IConfiguration ist das zentrale Interface in .NET, um Konfigurationswerte zur Laufzeit zu lesen
                    var configuration = configBuilder
                        .AddEnvironmentVariables()  // alle Umgebungsvariablen überlagern
                        .Build();                   // zu einer auflösbaren Konfiguration zusammenführen

                    // Konfigurationsobjekte aus den geladenen JSON-Dateien binden
                    services.Configure<ServicesObsConfiguration>(configuration.GetSection("Services"));
                    services.Configure<AuthenticationConfiguration>(configuration.GetSection("Authentication"));

                    // Mehrere Provider implementieren IEmployeesProvider.
                    // Der Store entscheidet dynamisch, welche Employees geladen werden
                    services.AddSingleton<IEmployeesProvider, EmployeesApiConfiguration>();
                    services.AddSingleton<IEmployeesProvider, EmployeesAppsettingsConfiguration>();

                    // Authentifizierungs- und Geschäftslogik-Services
                    services.AddSingleton<AuthenticationService>();
                    services.AddSingleton<EmployeeStore>();
                    services.AddSingleton<BookingService>();
                    services.AddSingleton<IObsBookingServiceAdapter, ObsBookingServiceAdapter>();

                    // registrierung des Options-Mechanismus im DI-Container.
                    // Ermöglicht Konfigurationen direkt in stark typisierte Klassen zu binden
                    // mit Validierung, Default-Werten und Zugriff via IOptions<T>
                    services.AddOptions();

                    // Hintergrunddienst, der zyklisch Buchungen ausführt
                    services.AddHostedService<Worker>();

                    // Dynamische Erstellung und Konfiguration der OBS API Clients
                    services.AddSingleton<IPersonsApi>(provider =>
                    {
                        try
                        {
                            // Lade URL für Personen-Service
                            var servicesConfig = provider.GetRequiredService<IOptions<ServicesObsConfiguration>>();
                            var obsStammUrl = servicesConfig.Value.StammServiceUrl;
                            var personsApi = new PersonsApi($"{obsStammUrl}");

                            // Hole AccessToken über AuthService
                            var authenticationService = provider.GetRequiredService<AuthenticationService>();
                            var accessToken = authenticationService.GetAccessTokenAsync(default).Result;

                            // Konfiguriere die OBS API mit dem Token
                            personsApi.Configuration = OBS.Stamm.Client.Client.Configuration.MergeConfigurations(
                                new OBS.Stamm.Client.Client.Configuration()
                                {
                                    AccessToken = accessToken
                                },
                                personsApi.Configuration);

                            return personsApi;
                        }
                        catch
                        {
                            // Bei Fehler: Rückfall auf Konfiguration über appsettings.json
                            Console.WriteLine("Configuration IPersonsApi is failed!\nFallback: using appsettings.json");
                            return null;
                        }
                    });

                    // Analog für Calendar API
                    services.AddSingleton<IPersonCalendarApi>(provider =>
                    {
                        try
                        {
                            var servicesConfig = provider.GetRequiredService<IOptions<ServicesObsConfiguration>>();
                            var obsCalendarUrl = servicesConfig.Value.CalendarServiceUrl;
                            var calendarApi = new PersonCalendarApi($"{obsCalendarUrl}");
                            var authenticationService = provider.GetRequiredService<AuthenticationService>();
                            var accessToken = authenticationService.GetAccessTokenAsync(default).Result;
                            calendarApi.Configuration = OBS.Calendar.Client.Client.Configuration.MergeConfigurations(
                            new OBS.Calendar.Client.Client.Configuration()
                            {
                                AccessToken = accessToken
                            },
                            calendarApi.Configuration);

                            return calendarApi;
                        }
                        catch
                        {
                            Console.WriteLine("Configuration IPersonCalendarApi is failed!\nFallback: using appsettings.json");
                            return null;
                        }
                    });

                    // Analog für Booking API
                    services.AddSingleton<IBookingApi>(provider =>
                    {
                        try
                        {
                            var servicesConfig = provider.GetRequiredService<IOptions<ServicesObsConfiguration>>();
                            var obsBookingUrl = servicesConfig.Value.BookingServiceUrl;
                            var bookingApi = new BookingApi($"{obsBookingUrl}");
                            var authenticationService = provider.GetRequiredService<AuthenticationService>();
                            var accessToken = authenticationService.GetAccessTokenAsync(default).Result;
                            bookingApi.Configuration = OBS.Booking.Client.Client.Configuration.MergeConfigurations(
                            new OBS.Booking.Client.Client.Configuration()
                            {
                                AccessToken = accessToken
                            },
                            bookingApi.Configuration);

                            return bookingApi;
                        }
                        catch
                        {
                            Console.WriteLine("Configuration IBookingApi is failed!");
                            return null;
                        }
                    });

                    // Hinweis bei fehlender NuGet.Config (z.B. beim Clonen von Repos)
                    if (!File.Exists("NuGet.Config"))
                    {
                        Console.WriteLine("WARNING: NuGet.Config not found. Private feeds may not work for cloned projects.");
                    }                    
                });
    }
}
