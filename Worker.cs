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
            IPersonsApi? stammApi,
            IBookingApi? bookingApi,
            IPersonCalendarApi? calenderApi,
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
            Console.WriteLine("\nBackgroundservice started: " + DateTime.Now);

            TimeSpan timeSpan = new TimeSpan(0, 1, 0);

            while (!stoppingToken.IsCancellationRequested)
            {
                if (employees.Count == 0 || DateTime.Now >= DateTime.Parse("00:00:00") && DateTime.Now <= DateTime.Parse("00:01:00"))
                {
                    Console.WriteLine($"Check employees: {DateTime.Now}");

                    if (_stammApi != null && _calenderApi != null)
                        employees = new EmployeesConfiguration(_stammApi, _calenderApi, _logger).Employees;

                    if (employees.Count < 8)
                    {

                    }

                    Console.WriteLine($"Registered employees: {employees.Count}\n");

                    foreach (Employee employee in employees)
                    {
                        Console.WriteLine($"Id: {employee.Id.PadRight(10, ' ')}Name: {employee.Name.PadRight(20, ' ')}" +
                            $"\tStart work: {employee.StartWork}\tEnd work: {employee.EndWork}");
                    }
                }

                if (employees.Count > 0)
                {
                    bookingService.ExecuteAsync(employees);
                }                

                await Task.Delay(60000, stoppingToken);
            }
        }
    }
}
