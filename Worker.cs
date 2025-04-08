using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OBS.Booking.Client.Api;
using OBS.Calendar.Client.Api;
using OBS.Stamm.Client.Api;
using OBS_Booking.Services.Configuration;
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
        private readonly EmployeesApiConfiguration _apiConfig;
        private readonly EmployeesAppsettingsConfiguration _appsettingsConfig;
        private readonly BookingService _bookingService;
        private readonly IPersonsApi? _stammApi;
        private readonly IPersonCalendarApi? _calenderApi;
        private readonly ILogger _logger;

        public Worker(
            EmployeesApiConfiguration apiConfig,
            EmployeesAppsettingsConfiguration appsettingsConfig,
            BookingService bookingService,
            IPersonsApi stammApi,
            IPersonCalendarApi calenderApi,
            ILogger<Worker> logger)
        {
            _apiConfig = apiConfig;
            _appsettingsConfig = appsettingsConfig;
            _bookingService = bookingService;
            _stammApi = stammApi;
            _calenderApi = calenderApi;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("\nBackgroundservice started: " + DateTime.Now);
            Console.WriteLine("\nBackgroundservice started: " + DateTime.Now);

            List<Employee> employees = new();
            TimeSpan timeSpan = new TimeSpan(0, 1, 0);

            while (!stoppingToken.IsCancellationRequested)
            {
                if (employees.Count == 0 || DateTime.Now >= DateTime.Parse("00:00:00") && DateTime.Now <= DateTime.Parse("00:01:00"))
                {
                    Console.WriteLine($"Check employees: {DateTime.Now}");

                    if (_stammApi != null && _calenderApi != null)
                        employees = _apiConfig.Employees;

                    if (employees.Count < 6)
                    {
                        var fallbackEmployees = _appsettingsConfig.Employees;

                        foreach (var emp in fallbackEmployees)
                        {
                            employees.Add(emp);
                        }
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
                    _bookingService.ExecuteAsync(employees);
                }                

                await Task.Delay(60000, stoppingToken);
            }
        }
    }
}
