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

namespace OBS_Booking_App
{
    public class Program
    {
        public static void Main(string[] args)
        {
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
                    var configuration = new ConfigurationBuilder()
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .AddJsonFile("appsettings.OBS.Configuration.json", optional: true, reloadOnChange: true)
                        .AddEnvironmentVariables()
                        .Build();

                    services.AddOptions();
                    services.AddTransient<AuthenticationService>();
                    services.AddHostedService<Worker>();

                    services.Configure<ServicesConfiguration>(configuration.GetSection("Services"));
                    services.Configure<AuthenticationConfiguration>(configuration.GetSection("Authentication"));

                    services.AddTransient<IPersonsApi>(provider =>
                    {
                        var servicesConfig = provider.GetRequiredService<IOptions<ServicesConfiguration>>();
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
                    });

                    services.AddTransient<IPersonCalendarApi>(provider =>
                    {
                        var servicesConfig = provider.GetRequiredService<IOptions<ServicesConfiguration>>();
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
                    });

                    services.AddTransient<IBookingApi>(provider =>
                    {
                        var servicesConfig = provider.GetRequiredService<IOptions<ServicesConfiguration>>();
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
                    });
                });
    }
}
