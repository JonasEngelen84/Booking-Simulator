using Microsoft.Extensions.Logging;
using OBS.Calendar.Client.Api;
using OBS.Calendar.Client.Model;
using OBS.Stamm.Client.Api;
using OBS.Stamm.Client.Model;
using OBS_Booking_App.Models;
using OBS_Booking_App.Services.API;
using System;
using System.Collections.Generic;

namespace OBS_Booking_App.Services.Configuration
{
    /// <summary>
    /// Implementiert <see cref="IEmployeesProvider"/>
    /// und lädt Mitarbeiterdaten aus ObsStamm und ObsCalendar.
    /// </summary>
    public class EmployeesApiConfiguration : IEmployeesProvider
    {
        List<Employee> employeesCache = new();

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

        /// <summary>
        /// Gibt eine Liste valider Mitarbeiter mit simulierten Buchungszeiten zurück.
        /// Fehlerhafte Einträge werden geloggt und ignoriert.
        /// </summary>
        public List<Employee> Employees
        {
            get
            {
                if (_stammApi != null || _calendarApi == null)
                    return employeesCache;

                foreach (var emp in _stammApi.All())
                {
                    try
                    {
                        ValidateObsStammData(emp);

                        var calendarEntries = _calendarApi.GetSimpleFromNumberAndDateAsync(emp.Id, DateTime.Now.Date.ToUniversalTime());
                        DateTime? startTime = null;
                        DateTime? endTime = null;
                        foreach (var entry in calendarEntries)
                        {
                            ValidateCalendarData(entry);
                            startTime = entry.StartTime;
                            endTime = entry.EndTime;
                        }

                        CreateEmployee(emp.Id, emp.Name, startTime, endTime);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Employee data failure: {ex.Message}");
                    }
                }

                return employeesCache;
            }
        }

        /// <summary>
        /// Prüft die Gültigkeit der Stammdaten einer Person.
        /// </summary>
        /// <param name="emp">Zu prüfender Personeneintrag</param>
        /// <exception cref="ArgumentException">Bei ungültiger ID oder Name</exception>
        private void ValidateObsStammData(SimplePersonApiModel emp)
        {
            if (string.IsNullOrWhiteSpace(emp.Id))
                throw new ArgumentException("Employee ID is invalid.");

            if (string.IsNullOrWhiteSpace(emp.Name))
                throw new ArgumentException("Employee name is invalid.");
        }

        /// <summary>
        /// Prüft Kalendereintrag auf gültige Arbeits-Beginn-/-End-Zeiten.
        /// </summary>
        /// <param name="entry">Kalendereintrag</param>
        /// <exception cref="ArgumentException">Bei fehlender Start-/Endzeit</exception>
        private void ValidateCalendarData(SimplePersonCalendarApiModel entry)
        {
            if (entry.StartTime == null || entry.EndTime == null)
                throw new ArgumentException($"Invalid StartTime or EndTime for employee: {entry.PersonId}");
        }

        private void CreateEmployee(string id, string name, DateTime? startWork, DateTime? endWork)
        {
            var (bookingStart, bookingEnd) = GenerateBookingTimes(startWork, endWork);

            employeesCache.Add(new Employee(
                    id,
                    name,
                    startWork,
                    endWork,
                    bookingStart,
                    bookingEnd)
            {
                LoggedIn = bookingStart <= DateTime.Now && bookingEnd >= DateTime.Now
            });
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
            int endOffset = rnd.Next(1, 10) <= 3 ? rnd.Next(0, 10) : rnd.Next(-10, 0);

            return (start.Value.AddMinutes(startOffset), end.Value.AddMinutes(endOffset));
        }
    }
}
