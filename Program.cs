using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OBS.Stamm.Client.Api;
using OBS_Booking_App.Services;
using OBS_Booking_App.Services.Configuration;
using Serilog;
using System;
using OBS.LIB.Logging.Extensions;
using OBS.Calendar.Client.Api;
using OBS.Booking.Client.Api;
using System.IO;
using OBS_Booking.Services.Configuration;
using OBS_Booking_App.Services.API;
using OBS_Booking_App.Stores;

namespace OBS_Booking_App
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Booking Simulator started\n\n");

            try
            {
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
                .AddObsLogging()
                .ConfigureServices((hostContext, services) =>
                {
                    var configBuilder = new ConfigurationBuilder()
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

                    var obsConfigPath = "appsettings.OBS.Configuration.json";
                    var useObsApi = File.Exists(obsConfigPath);
                    if (useObsApi)
                    {
                        configBuilder.AddJsonFile(obsConfigPath, optional: true, reloadOnChange: true);
                    }

                    var configuration = configBuilder
                        .AddEnvironmentVariables()
                        .Build();

                    services.AddOptions();
                    services.AddSingleton<EmployeesApiConfiguration>();
                    services.AddSingleton<EmployeesAppsettingsConfiguration>();
                    services.AddSingleton<AuthenticationService>();
                    services.AddSingleton<BookingService>();
                    services.AddSingleton<EmployeeStore>();
                    services.AddHostedService<Worker>();

                    services.Configure<ServicesObsConfiguration>(configuration.GetSection("Services"));
                    services.Configure<AuthenticationConfiguration>(configuration.GetSection("Authentication"));

                    services.AddSingleton<IPersonsApi>(provider =>
                    {
                        try
                        {
                            var servicesConfig = provider.GetRequiredService<IOptions<ServicesObsConfiguration>>();
                            var obsStammUrl = servicesConfig.Value.StammServiceUrl;
                            var personsApi = new PersonsApi($"{obsStammUrl}");

                            var authenticationService = provider.GetRequiredService<AuthenticationService>();
                            var accessToken = authenticationService.GetAccessTokenAsync(default).Result;
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
                            Console.WriteLine("Configuration IPersonsApi is failed!\nFallback: using appsettings.json");
                            return null;
                        }
                    });

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

                    if (!File.Exists("NuGet.Config"))
                    {
                        Console.WriteLine("WARNING: NuGet.Config not found. Private feeds may not work for cloned projects.");
                    }                    
                });
    }
}
