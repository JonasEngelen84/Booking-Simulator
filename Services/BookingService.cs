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

        private bool bookingServiceStarted = true;
        private bool booking = false;

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

            foreach (Employee employee in employees)
            {
                DateTime timeNow = DateTime.Now;

                if (employee.BookingStartWork >= timeNow && employee.BookingStartWork <= timeNow.AddMinutes(1))
                {
                    await LoggingProcess(employee, true, "logged IN");
                }

                if (employee.BookingEndWork >= timeNow && employee.BookingEndWork <= timeNow.AddMinutes(1))
                {
                    await LoggingProcess(employee,false, "logged OUT");
                    _employeeStore.Employees.Remove(employee);
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

        private async Task LoggingProcess(Employee employee, bool logged, string log)
        {
            if (bookingServiceStarted)
            {
                Console.WriteLine("\nBooking Service started:");
                bookingServiceStarted = false;
            }

            if (_bookingApi != null)
            {
                CreateBookingModel bookingObj;

                if (logged)
                    bookingObj = new(0, BookingType.ARRIVE, DateTime.Now, default, employee.Id, null);
                else
                    bookingObj = new(0, BookingType.LEAVE, DateTime.Now, default, employee.Id, null);

                await _bookingApi.CreateAsync(bookingObj);
            }                

            employee.LoggedIn = logged;
            _logger.LogInformation($"Id: {employee.Id,-8}Name: {employee.Name,-20}{log}: {DateTime.Now}");
            Console.WriteLine($"Id: {employee.Id,-8}Name: {employee.Name,-20}{log}: {DateTime.Now}");
            booking = true;
            await Task.Delay(3000);
        }
    }
}
