using Microsoft.Extensions.Logging;
using OBS.Booking.Client.Api;
using OBS.Booking.Client.Model;
using OBS_Booking_App.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OBS_Booking_App.Services.Configuration
{
    public class BookingService
    {
        private readonly IBookingApi? _bookingApi;
        private readonly ILogger<BookingService> _logger;
        bool booking;

        public BookingService(IBookingApi bookingApi, ILogger<BookingService> logger)
        {
            _bookingApi = bookingApi;
            _logger = logger;
        }

        public async Task ExecuteAsync(List<Employee> employees)
        {
            _logger.LogInformation($"\nBooking process started: {DateTime.Now}");
            Console.WriteLine($"\nBooking process started: {DateTime.Now}");

            TimeSpan TimeSpan = new TimeSpan(0, 1, 0);

            foreach (Employee employee in employees)
            {
                // Liegt StartWork innerhalb der Aktuellen Zeit und aktuelle Zeit + 1 Minute
                if (employee.BookingStartWork <= DateTime.Now && employee.BookingStartWork >= DateTime.Now.Add(TimeSpan))
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
                }

                // Liegt EndWork innerhalb der Aktuellen Zeit und aktuelle Zeit + 1 Minute
                if (employee.EndWork <= DateTime.Now && employee.EndWork >= DateTime.Now.Add(TimeSpan))
                {
                    if (_bookingApi != null)
                    {
                        CreateBookingModel bookingObj = new(0, BookingType.ARRIVE, DateTime.Now, default, employee.Id, null);
                        _bookingApi.Create(bookingObj);
                    }

                    booking = true;
                    _logger.LogInformation($"\n{employee.Name}     \tId: {employee.Id} \tLogged OUT: {DateTime.Now}");
                    Console.WriteLine($"\n{employee.Name}     \tId: {employee.Id} \tLogged OUT: {DateTime.Now}");
                }

                Thread.Sleep(3000);
            }

            if (booking == true)
            {
                int loggedIn = 0;

                _logger.LogInformation("\n\nActually logged in:");
                Console.WriteLine("\n\nActually logged in:");

                foreach (Employee employee in employees)
                {
                    if (employee.LoggedIn)
                    {
                        _logger.LogInformation($"{employee.Name.PadLeft(16, ' ')}\tId: {employee.Id.PadLeft(6, ' ')}\tStatus: Logged IN");
                        Console.WriteLine($"{employee.Name.PadLeft(16, ' ')}\tId: {employee.Id.PadLeft(6, ' ')}\tStatus: Logged IN");

                        loggedIn++;
                    }
                }
                _logger.LogInformation($"{loggedIn} Employees are Logged in: {DateTime.Now}\n\nBooking done\n");
                Console.WriteLine($"{loggedIn} Employees are Logged in: {DateTime.Now}\n\nBooking done\n");
            }
        }
    }
}
