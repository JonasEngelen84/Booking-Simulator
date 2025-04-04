using Microsoft.Extensions.Logging;
using OBS.Booking.Client.Api;
using OBS.Booking.Client.Model;
using OBS_Booking_App.Services;
using OBS_Booking_App.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OBS_Booking_App.Services.Configuration
{
    public class BookingService
    {
        private readonly IBookingApi _bookingApi;
        private readonly ILogger _logger;
        private readonly Worker _worker;
        List<Employee> removeFromEmployeesList = new();
        bool booking;

        public BookingService(IBookingApi bookingApi, ILogger logger, Worker worker)
        {
            _bookingApi = bookingApi;
            _logger = logger;
            _worker = worker;
        }

        public async Task ExecuteAsync(List<Employee> employees)
        {
            _logger.LogInformation($"\nBooking process started: {DateTime.Now}");
            Console.WriteLine($"\nBooking process started: {DateTime.Now}");

            TimeSpan TimeSpan = new TimeSpan(0, 1, 0);

            foreach (Employee employee in employees)
            {
                // Liegt StartWork innerhalb der Aktuellen Zeit und aktuelle Zeit + 1 Minute
                if (employee.StartWork <= DateTime.Now && employee.StartWork >= DateTime.Now.Add(TimeSpan))
                {
                    CreateBookingModel bookingObj = new(0, BookingType.ARRIVE, DateTime.Now, default, employee.Id, null);
                    _bookingApi.Create(bookingObj);
                    employee.LoggedIn = true;
                    booking = true;
                    _logger.LogInformation($"\n{employee.Name}     \tId: {employee.Id} \tLogged IN: {DateTime.Now}");
                    Console.WriteLine($"\n{employee.Name}     \tId: {employee.Id} \tLogged IN: {DateTime.Now}");
                }

                // Liegt EndWork innerhalb der Aktuellen Zeit und aktuelle Zeit + 1 Minute
                if (employee.EndWork <= DateTime.Now && employee.EndWork >= DateTime.Now.Add(TimeSpan))
                {
                    CreateBookingModel bookingObj = new(0, BookingType.LEAVE, DateTime.Now, default, employee.Id, null);
                    _bookingApi.Create(bookingObj);
                    removeFromEmployeesList.Add(employee);
                    booking = true;
                    _logger.LogInformation($"\n{employee.Name}     \tId: {employee.Id} \tLogged OUT: {DateTime.Now}");
                    Console.WriteLine($"\n{employee.Name}     \tId: {employee.Id} \tLogged OUT: {DateTime.Now}");
                }

                Thread.Sleep(3000);
            }

            // Wenn gebucht wurde => zeige Anwesenheitsliste.
            if (booking == true)
            {
                outputEmployeesList(employees);
            }
        }

        // Mitarbeiter-Liste ausgeben.
        public void outputEmployeesList(List<Employee> employees)
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
            _logger.LogInformation($"{loggedIn} Employees are Logged: {DateTime.Now}\n\nBooking done\n");
            Console.WriteLine($"{loggedIn} Employees are Logged: {DateTime.Now}\n\nBooking done\n");
        }
    }
}
