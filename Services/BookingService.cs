using Microsoft.Extensions.Logging;
using OBS.Booking.Client.Api;
using OBS.Booking.Client.Model;
using OBS_Booking_App.Models;
using OBS_Booking_App.Stores;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OBS_Booking_App.Services.Configuration
{
    public class BookingService
    {
        private readonly EmployeeStore _employeeStore;
        private readonly IBookingApi? _bookingApi;
        private readonly ILogger<BookingService> _logger;
        private bool booking;

        public BookingService(
            EmployeeStore employeesStore,
            IBookingApi bookingApi,
            ILogger<BookingService> logger)
        {
            _employeeStore = employeesStore;
            _bookingApi = bookingApi;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            _logger.LogInformation($"\nBooking process started: {DateTime.Now}");
            Console.WriteLine($"\nBooking process started: {DateTime.Now}");

            foreach (Employee employee in _employeeStore.Employees)
            {
                // Liegt StartWork innerhalb der Aktuellen Zeit und aktuelle Zeit + 1 Minute
                if (employee.BookingStartWork >= DateTime.Now && employee.BookingStartWork <= DateTime.Now.AddMinutes(1))
                {
                    if (_bookingApi != null)
                    {
                        CreateBookingModel bookingObj = new(0, BookingType.ARRIVE, DateTime.Now, default, employee.Id, null);
                        _bookingApi.Create(bookingObj);
                    }

                    employee.LoggedIn = true;
                    booking = true;
                    _logger.LogInformation($"\n{employee.Name}     \tId: {employee.Id} \tLogged IN: {DateTime.Now}");
                    Console.WriteLine($"\n{employee.Name}     \tId: {employee.Id} \tLogged IN: {DateTime.Now}");
                    Thread.Sleep(3000);
                }

                // Liegt EndWork innerhalb der Aktuellen Zeit und aktuelle Zeit + 1 Minute
                if (employee.BookingEndWork <= DateTime.Now && employee.BookingEndWork >= DateTime.Now.AddMinutes(1))
                {
                    if (_bookingApi != null)
                    {
                        CreateBookingModel bookingObj = new(0, BookingType.ARRIVE, DateTime.Now, default, employee.Id, null);
                        _bookingApi.Create(bookingObj);
                    }

                    employee.LoggedIn = false;
                    booking = true;
                    _employeeStore.Employees.Remove(employee);
                    _logger.LogInformation($"\n{employee.Name}     \tId: {employee.Id} \tLogged OUT: {DateTime.Now}");
                    Console.WriteLine($"\n{employee.Name}     \tId: {employee.Id} \tLogged OUT: {DateTime.Now}");
                    Thread.Sleep(3000);
                }
            }

            if (booking == true)
            {
                int loggedIn = 0;

                _logger.LogInformation("\n\nActually logged in:");
                Console.WriteLine("\n\nActually logged in:");

                foreach (Employee employee in _employeeStore.Employees)
                {
                    if (employee.LoggedIn)
                    {
                        _logger.LogInformation($"{employee.Name.PadLeft(16, ' ')}\tId: {employee.Id.PadLeft(6, ' ')}\tStatus: Logged IN");
                        Console.WriteLine($"{employee.Name.PadLeft(16, ' ')}\tId: {employee.Id.PadLeft(6, ' ')}\tStatus: Logged IN");

                        loggedIn++;
                    }
                }

                _logger.LogInformation($"{loggedIn} Employees logged in: {DateTime.Now}");
                Console.WriteLine($"{loggedIn} Employees logged in: {DateTime.Now}");
            }
        }
    }
}
