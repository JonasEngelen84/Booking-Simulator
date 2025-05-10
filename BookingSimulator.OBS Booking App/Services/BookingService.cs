using Microsoft.Extensions.Logging;
using OBS.Booking.Client.Api;
using OBS.Booking.Client.Model;
using OBS_Booking_App.Models;
using OBS_Booking_App.Services.API;
using OBS_Booking_App.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OBS_Booking_App.Services.Configuration
{
    public class BookingService
    {
        private readonly EmployeeStore _employeeStore;
        private readonly IObsBookingServiceAdapter _obsBookingAdapter;
        private readonly ILogger<BookingService> _logger;

        private bool bookingServiceStarted = true;
        private bool booking = false;

        public BookingService(
            EmployeeStore employeesStore,
            IObsBookingServiceAdapter obsBookingAdapter,
            ILogger<BookingService> logger)
        {
            _employeeStore = employeesStore;
            _obsBookingAdapter = obsBookingAdapter;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            bookingServiceStarted = true;
            List<Employee> employees = new(_employeeStore.Employees);

            foreach (Employee employee in employees)
            {
                if (employee.BookingStartWork >= DateTime.Now && employee.BookingStartWork <= DateTime.Now.AddMinutes(1))
                {
                    await LoggingProcessAsync(employee, true, "logged IN");
                }

                if (employee.BookingEndWork >= DateTime.Now && employee.BookingEndWork <= DateTime.Now.AddMinutes(1))
                {
                    await LoggingProcessAsync(employee, false, "logged OUT");
                    _employeeStore.Employees.Remove(employee);
                }
            }

            if (booking)
            {
                Console.WriteLine($"\n{DateTime.Now}   logged in: {employees.Count(e => e.LoggedIn)}");
                DisplayActuallyLoggedInEmployees();
                booking = false;
            }
        }

        private async Task LoggingProcessAsync(Employee employee, bool logged, string log)
        {
            if (bookingServiceStarted)
            {
                Console.WriteLine("\nBooking Service started:");
                bookingServiceStarted = false;
            }

            try
            {
                await _obsBookingAdapter.CreateBookingAsync(employee, logged ? BookingAction.Arrive : BookingAction.Leave);
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Connection to IBookingApi failed."+ ex.ToString());
                Console.WriteLine("Connection to IBookingApi failed.");
            }

            employee.LoggedIn = logged;
            _logger.LogInformation($"Id: {employee.Id,-8}Name: {employee.Name,-20}{log}: {DateTime.Now}");
            Console.WriteLine($"Id: {employee.Id,-8}Name: {employee.Name,-20}{log}: {DateTime.Now}");
            booking = true;
            await Task.Delay(3000);
        }

        private void DisplayActuallyLoggedInEmployees()
        {
            Console.WriteLine($"\n{DateTime.Now}    Logged in: {_employeeStore.Employees.Count(e => e.LoggedIn)}");

            foreach (Employee employee in _employeeStore.Employees)
            {
                if (employee.LoggedIn)
                {
                    Console.WriteLine($"Id: {employee.Id,-10}Name: {employee.Name}");
                }
            }
        }
    }
}
