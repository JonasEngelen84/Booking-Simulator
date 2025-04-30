using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OBS_Booking_App.Models;
using OBS_Booking_App.Services.Configuration;
using OBS_Booking_App.Stores;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OBS_Booking_App
{
    public class Worker : BackgroundService
    {
        private EmployeeStore _employeeStore { get; }
        private BookingService _bookingService { get; }
        private ILogger _logger { get; }

        public Worker(EmployeeStore employeesStore, BookingService bookingService, ILogger<Worker> logger)
        {
            _employeeStore = employeesStore;
            _bookingService = bookingService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"{DateTime.Now} Backgroundservice started");
            Console.WriteLine($"{DateTime.Now} Backgroundservice started");

            while (!stoppingToken.IsCancellationRequested)
            {
                if (_employeeStore.Employees.Count == 0 || (DateTime.Now >= DateTime.Parse("01:06:00") && DateTime.Now <= DateTime.Parse("01:07:00")))
                {
                    _employeeStore.UpdateEmployees();
                    DisplayRegisteredEmployees();
                    DisplayActuallyLoggedInEmployees();
                }

                long elapsedMilliseconds = 0;
                if (_employeeStore.Employees.Count > 0)
                {
                    var stopwatch = Stopwatch.StartNew();
                    await _bookingService.ExecuteAsync();
                    stopwatch.Stop();
                    elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
                }

                var delay = Math.Max(0, 60000 - elapsedMilliseconds);
                await Task.Delay((int)delay, stoppingToken);
            }
        }

        private void DisplayRegisteredEmployees()
        {
            Console.WriteLine($"Registered employees: {_employeeStore.Employees.Count}");
            foreach (Employee employee in _employeeStore.Employees)
            {
                Console.WriteLine($"| Id: {employee.Id,-10} | Name: {employee.Name,-20} | Start work: {employee.StartWork,-22} | End work: {employee.EndWork} |");
            }
        }

        private void DisplayActuallyLoggedInEmployees()
        {
            _logger.LogInformation($"\n{DateTime.Now} actually logged in: {_employeeStore.Employees.Count(e => e.LoggedIn)}");
            Console.WriteLine($"\n{DateTime.Now} actually logged in: {_employeeStore.Employees.Count(e => e.LoggedIn)}");

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
