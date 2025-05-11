using Microsoft.Extensions.Logging;
using OBS_Booking.App.Models;
using OBS_Booking.App.Services.API;
using OBS_Booking.App.Stores;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OBS_Booking.App.Services.Configuration
{
    /// <summary>
    /// Zentraler Service zur automatisierten Buchung von Mitarbeiteranwesenheiten über externe OBS-Services.
    /// Das Interface <see cref="IObsBookingAdapter"/> abstrahiert die externe OBS-Buchungslogik, 
    /// um die Testbarkeit zu erhöhen, konkrete Abhängigkeiten zu entkoppeln und alternative Implementierungen zu ermöglichen.
    /// </summary>
    public class BookingService
    {
        private readonly EmployeeStore _employeeStore;
        private readonly IObsBookingAdapter _obsBookingAdapter;
        private readonly ILogger<BookingService> _logger;

        private bool _bookingServiceStarted = true;
        private bool _bookingOccurred = false;

        public BookingService(
            EmployeeStore employeesStore,
            IObsBookingAdapter obsBookingAdapter,
            ILogger<BookingService> logger)
        {
            _employeeStore = employeesStore;
            _obsBookingAdapter = obsBookingAdapter;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            _bookingServiceStarted = true;
            List<Employee> employeesSnapshot = new(_employeeStore.Employees);

            foreach (Employee employee in employeesSnapshot)
            {
                // Prüft ob eine Ankunftsbuchung in der nächsten Minute erfolgen soll
                if (employee.BookingStartWork >= DateTime.Now && employee.BookingStartWork <= DateTime.Now.AddMinutes(1))
                {
                    await LoggingProcessAsync(employee, true, "logged IN");
                }

                // Prüft ob eine Abgangsbuchung in der nächsten Minute erfolgen soll
                if (employee.BookingEndWork >= DateTime.Now && employee.BookingEndWork <= DateTime.Now.AddMinutes(1))
                {
                    await LoggingProcessAsync(employee, false, "logged OUT");
                    _employeeStore.Employees.Remove(employee);
                }
            }

            // Wenn gebucht wurde => eingeloggte Mitarbeiter ausgeben
            if (_bookingOccurred)
            {
                EmployeeDisplayService.DisplayLoggedInEmployees(_employeeStore.Employees);
                _bookingOccurred = false;
            }
        }

        /// <summary>
        /// Führt eine Buchung (Ankunft oder Abgang) durch und protokolliert sie.
        /// </summary>
        /// <param name="isLogin">true für Ankunft, false für Abgang</param>
        /// <param name="logText">Beschreibung für die Konsole</param>
        private async Task LoggingProcessAsync(Employee employee, bool isLogin, string logText)
        {
            if (_bookingServiceStarted)
            {
                Console.WriteLine("\nBooking Service started:");
                _bookingServiceStarted = false;
            }

            Console.WriteLine($"Id: {employee.Id,-10}Name: {employee.Name,-25}{logText}: {DateTime.Now}");
            employee.LoggedIn = isLogin;
            _bookingOccurred = true;

            try
            {
                // Durch Dependency Injection wird beim Hosting die konkrete Implementierung ObsBookingAdapterService injiziert.
                // Die Methode CreateBookingAsync wird darüber aufgerufen.
                // Das Enum BookingAction ist in derselben Namespace-Hierarchie wie das Interface IObsBookingAdapter
                // deklariert und wird dort als Parameter verwendet. So kann es direkt mit dem Interface genutzt werden.
                await _obsBookingAdapter.CreateBookingAsync(employee.Id, isLogin ? BookingAction.Arrive : BookingAction.Leave);
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Connection to BookingApi failed."+ ex.ToString());
                Console.WriteLine("Connection to BookingApi failed.");
            }

            await Task.Delay(3000); // Wartet 3 Sekunden um API-Überlastung zu vermeiden + Realitätsnähe
        }
    }
}
