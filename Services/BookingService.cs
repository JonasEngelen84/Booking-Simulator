using Microsoft.Extensions.Logging;
using OBS.Booking.Client.Api;
using OBS.Booking.Client.Model;
using OBS_Booking_App.Models;
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
            List<Employee> employees = new(_employeeStore.Employees);
            int count = 0;

            foreach (Employee employee in employees)
            {
                DateTime timeNow = DateTime.Now;

                if (employee.BookingStartWork >= timeNow && employee.BookingStartWork <= timeNow.AddMinutes(1))
                {
                    if (count == 0)
                    {
                        Console.WriteLine("\nBooking Service started:");
                        count++;
                    }

                    if (_bookingApi != null)
                    {
                        CreateBookingModel bookingObj = new(0, BookingType.ARRIVE, timeNow, default, employee.Id, null);
                        _bookingApi.Create(bookingObj);
                    }

                    employee.LoggedIn = true;
                    booking = true;
                    _logger.LogInformation($"Id: {employee.Id,-8}Name: {employee.Name,-20} log IN: {timeNow}");
                    Console.WriteLine($"Id: {employee.Id,-8}Name: {employee.Name,-20} log IN: {timeNow}");
                    Thread.Sleep(3000);
                }

                if (employee.BookingEndWork >= timeNow && employee.BookingEndWork <= timeNow.AddMinutes(1))
                {
                    if (count == 0)
                    {
                        Console.WriteLine("\nBooking Service started:");
                        count++;
                    }

                    if (_bookingApi != null)
                    {
                        CreateBookingModel bookingObj = new(0, BookingType.ARRIVE, timeNow, default, employee.Id, null);
                        _bookingApi.Create(bookingObj);
                    }

                    employee.LoggedIn = false;
                    booking = true;
                    _employeeStore.Employees.Remove(employee);
                    _logger.LogInformation($"Id: {employee.Id,-8}Name: {employee.Name,-20}log OUT: {timeNow}");
                    Console.WriteLine($"Id: {employee.Id,-8}Name: {employee.Name,-20}log OUT: {timeNow}");
                    Thread.Sleep(3000);
                }
            }

            if (booking == true)
            {
                _logger.LogInformation($"\n{DateTime.Now} logged in: {employees.Count(e => e.LoggedIn)}");
                Console.WriteLine($"\n{DateTime.Now}   logged in: {employees.Count(e => e.LoggedIn)}");

                foreach (Employee employee in _employeeStore.Employees)
                {
                    if (employee.LoggedIn)
                    {
                        Console.WriteLine($"Id: {employee.Id}   Name: {employee.Name}");
                    }
                }

                booking = false;
            }
        }
    }
}
