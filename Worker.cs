using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OBS.Booking.Client.Api;
using OBS.Calendar.Client.Api;
using OBS.Stamm.Client.Api;
using OBS_Booking_App.Models;
using OBS_Booking_App.Services;
using OBS_Booking_App.Services.Configuration;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AuthenticationService = OBS_Booking_App.Services.AuthenticationService;

namespace OBS_Booking_App
{
    public class Worker : BackgroundService
    {
        private readonly IPersonsApi _stammApi;
        private readonly IPersonCalendarApi _calenderApi;
        private readonly IBookingApi _bookingApi;
        private readonly ILogger _logger;
        private readonly BookingService bookingService;

        private List<Employee> employees = new();

        public Worker(
            IPersonsApi stammApi,
            IBookingApi bookingApi,
            IPersonCalendarApi calenderApi,
            ILogger<Worker> logger)
        {
            _stammApi = stammApi;
            _calenderApi = calenderApi;
            _bookingApi = bookingApi;
            _logger = logger;
            bookingService = new BookingService(bookingApi, _logger, this);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("\nBackgroundservice started: " + DateTime.Now);
            Console.WriteLine("\nBooking Simulator\n\nBackgroundservice started: " + DateTime.Now);
            TimeSpan timeSpan = new TimeSpan(0, 1, 0);

            while (!stoppingToken.IsCancellationRequested)
            {
                // Um Mitternacht oder wenn die Mitarbeiter-Liste leer ist => Mitarbeiter-Liste updaten.
                if (employees.Count == 0 || DateTime.Now >= DateTime.Parse("00:00:00") && DateTime.Now <= DateTime.Parse("00:01:00"))
                {
                    employees = new EmployeesConfiguration(_stammApi, _calenderApi, _bookingApi, _logger).Employees;

                    Console.WriteLine($"\nRegistered employees: {employees.Count}\n");

                    foreach (Employee employee in employees)
                    {
                        Console.WriteLine($"Id: {employee.Id.PadRight(7, ' ')}Name: {employee.Name.PadRight(15, ' ')}" +
                            $"\tStart work: {employee.StartWork}\tEnd work: {employee.EndWork}");
                    }
                }

                bookingService.ExecuteAsync(employees);
                await Task.Delay(60000, stoppingToken);
            }
        }
    }
}
