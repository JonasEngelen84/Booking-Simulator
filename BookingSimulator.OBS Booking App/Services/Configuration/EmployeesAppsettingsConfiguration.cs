using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OBS_Booking.Services.Configuration;
using OBS_Booking_App.Models;
using OBS_Booking_App.Services.API;
using System;
using System.Collections.Generic;

namespace OBS_Booking_App.Services.Configuration
{
    /// <summary>
    /// Liest und verarbeitet employee-Daten aus appsettings.json.
    /// </summary>
    public class EmployeesAppsettingsConfiguration : IEmployeesProvider
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmployeesAppsettingsConfiguration> _logger;

        public EmployeesAppsettingsConfiguration(IConfiguration configuration, ILogger<EmployeesAppsettingsConfiguration> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public List<Employee> Employees
        {
            get
            {
                // Employees aus appsettings.json laden
                var employeesConfigs = _configuration.GetSection("Employees").Get<List<EmployeeConfiguration>>();
                List<Employee> employeesCache = [];

                foreach (var config in employeesConfigs)
                {
                    try
                    {
                        var employee = CreateEmployee(config);
                        employeesCache.Add(employee);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Invalid employee appsettings configuration: {config?.Id}   {config?.Name}  Error:\n{ex.Message}");
                    }
                }
                return employeesCache;
            }
        }

        private Employee CreateEmployee(EmployeeConfiguration config)
        {
            var startWork = config.StartWork;

            // Sonderfall: Eine Nachtschicht vom Vortag erstellen!
            if (startWork.TimeOfDay == new TimeSpan(22, 0, 1))
            {
                startWork = startWork.AddDays(-1);
            }

            var endWork = startWork.AddHours(8);
            var (bookingStartWork, bookingEndWork) = GenerateBookingTimes(startWork, endWork);

            return new Employee(
                config.Id,
                config.Name,
                startWork,
                endWork,
                bookingStartWork,
                bookingEndWork)
            {
                LoggedIn = bookingStartWork <= DateTime.Now && bookingEndWork >= DateTime.Now
            };
        }

        /// <summary>
        /// Generiert realistische Buchungszeitpunkte basierend auf den Kalenderzeiten.
        /// Abweichung erfolgt zufällig um einige Minuten.
        /// </summary>
        /// <param name="start">Originaler Arbeitsbeginn</param>
        /// <param name="end">Originales Arbeitsende</param>
        /// <returns>Tuple mit verschobenem Start- und Endzeitpunkt</returns>
        private (DateTime bookingStart, DateTime bookingEnd) GenerateBookingTimes(DateTime? start, DateTime? end)
        {
            var rnd = new Random();

            int startOffset = rnd.Next(1, 10) == 1 ? rnd.Next(0, 10) : rnd.Next(-10, 0);
            int endOffset = rnd.Next(0, 10);
            //return (DateTime.Now.AddMinutes(startOffset), DateTime.Now.AddMinutes(endOffset));
            return (start.Value.AddMinutes(startOffset), end.Value.AddMinutes(endOffset));
        }
    }
}
