using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OBS_Booking_App.Models;
using OBS_Booking_App.Services;
using OBS_Booking_App.Services.Configuration;
using OBS_Booking_App.Stores;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OBS_Booking_App
{
    /// <summary>
    /// Hintergrunddienst, der in regelmäßigen Abständen Buchungen verarbeitet und Mitarbeiterdaten aktualisiert.
    /// </summary>
    public class Worker : BackgroundService
    {
        private EmployeeStore _employeeStore { get; }
        private BookingService _bookingService { get; }
        private ILogger _logger { get; }

        public Worker(EmployeeStore employeesStore, BookingService bookingService, ILogger<Worker> logger)
        {
            _employeeStore = employeesStore;
            _bookingService = bookingService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("Backgroundservice started");

            while (!stoppingToken.IsCancellationRequested)
            {
                if (_employeeStore.Employees.Count == 0 || (DateTime.Now >= DateTime.Parse("00:00:00") && DateTime.Now <= DateTime.Parse("00:01:00")))
                {
                    Console.WriteLine($"\n{DateTime.Now} Update employees\nTry using OBS.API.Configuration");
                    _employeeStore.UpdateEmployees();
                    EmployeeDisplayService.DisplayRegisteredEmployees(_employeeStore.Employees);
                    EmployeeDisplayService.DisplayActuallyLoggedInEmployees(_employeeStore.Employees);
                }

                long elapsedMilliseconds = 0;

                /// <summary>
                /// Führt die Buchungslogik nur aus, wenn Mitarbeitende vorhanden sind.
                /// Die Ausführungszeit wird dabei exakt gemessen,
                /// um eine verzögerte Ausführung (Delay) auf exakt 60 Sekunden zu kalibrieren.
                /// </summary>
                if (_employeeStore.Employees.Count > 0)
                {
                    var stopwatch = Stopwatch.StartNew();                   // Startet eine Stoppuhr zur Messung der Ausführungsdauer
                    await _bookingService.ExecuteAsync();                   // Führt die Buchungsoperationen asynchron aus
                    stopwatch.Stop();                                       // Beendet die Zeitmessung
                    elapsedMilliseconds = stopwatch.ElapsedMilliseconds;    // Speichert die gemessene Ausführungszeit in Millisekunden
                }
                                
                var delay = Math.Max(0, 60000 - elapsedMilliseconds);       // Die gemessene Zeit wird von dem 60 Sekunden delay abgezogen
                await Task.Delay((int)delay, stoppingToken);                // Delay wird mit der Restzeit gestartet
            }
        }
    }
}
