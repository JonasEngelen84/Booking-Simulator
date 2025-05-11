using Microsoft.Extensions.Logging;
using OBS.Calendar.Client.Api;
using OBS.Stamm.Client.Api;
using OBS.Stamm.Client.Model;
using OBS_Booking.App.Models;
using OBS_Booking.App.Services.API;
using System;
using System.Collections.Generic;

namespace OBS_Booking.App.Services.Configuration
{
    /// <summary>
    /// Lädt Mitarbeiterdaten aus ObsStamm und ObsCalendar.
    /// </summary>
    public class EmployeesApiConfiguration : IEmployeesProvider
    {
        private readonly IPersonsApi _stammApi;
        private readonly IPersonCalendarApi _calendarApi;
        private readonly ILogger<EmployeesApiConfiguration> _logger;

        public EmployeesApiConfiguration(
            IPersonsApi stammApi,
            IPersonCalendarApi calenderApi,
            ILogger<EmployeesApiConfiguration> logger)
        {
            _stammApi = stammApi;
            _calendarApi = calenderApi;
            _logger = logger;
        }

        public List<Employee> Employees
        {
            get
            {
                List<Employee> employeesCache = [];

                if (_stammApi == null || _calendarApi == null)
                    return employeesCache;

                try
                {
                    // Employee-Daten aus OBS_Stamm laden
                    foreach (var spam in _stammApi.All())
                    {
                        ValidateObsStammData(spam);

                        // Employee-Daten aus OBS_Calendar laden
                        foreach (var dpcam in _calendarApi.GetDetailedFromNumberAndDateAsync(spam.Id, DateTime.Now.Date.ToUniversalTime()))
                        {
                            if (dpcam.StartTime == null || dpcam.EndTime == null)
                                throw new ArgumentException($"Invalid StartTime or EndTime for employee: {dpcam.PersonId}");

                            var employee = CreateEmployee(spam.Id, spam.Name, dpcam.StartTime, dpcam.EndTime);
                            employeesCache.Add(employee);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Employee data failure: {ex.Message}");
                }

                return employeesCache;
            }
        }

        /// <summary>
        /// Prüft die Gültigkeit der Stammdaten eines employee.
        /// </summary>
        /// <param name="spam">Zu prüfender Personeneintrag</param>
        /// <exception cref="ArgumentException">Bei ungültiger ID oder Name</exception>
        private void ValidateObsStammData(SimplePersonApiModel spam)
        {
            if (string.IsNullOrWhiteSpace(spam.Id))
                throw new ArgumentException("Employee ID is invalid.");

            if (string.IsNullOrWhiteSpace(spam.Name))
                throw new ArgumentException("Employee name is invalid.");

            //if (emp.DateOfEntry == null || emp.DateOfEntry <= DateTime.Now.Date)
            //    throw new ArgumentException($"Invalid entry date for employee: {emp.Name}");

            //if (emp.DateOfLeaving == null || emp.DateOfLeaving > DateTime.Now.Date)
            //    throw new ArgumentException($"Invalid leaving date for employee: {emp.Name}");
        }

        private Employee CreateEmployee(string id, string name, DateTime? startWork, DateTime? endWork)
        {
            var (bookingStartWork, bookingEndWork) = GenerateBookingTimes(startWork, endWork);

            return new Employee(
                    id,
                    name,
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

            return (DateTime.Now.AddMinutes(startOffset+10), DateTime.Now.AddMinutes(endOffset+15));
            //return (start.Value.AddMinutes(startOffset), end.Value.AddMinutes(endOffset));
        }
    }
}
