using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OBS_Booking_App.Models;
using OBS_Booking_App.Services.Configuration;
using OBS_Booking_App.Stores;
using System;
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
            _logger.LogInformation("\nBackgroundservice started: " + DateTime.Now);
            Console.WriteLine("\nBackgroundservice started: " + DateTime.Now);

            while (!stoppingToken.IsCancellationRequested)
            {
                if (_employeeStore.Employees.Count == 0 ||
                    DateTime.Now >= DateTime.Parse("00:00:00") && DateTime.Now <= DateTime.Parse("00:01:00"))
                {
                    _employeeStore.UpdateEmployees();
                    DisplayRegisteredEmployees();
                    DisplayActuallyLoggedInEmployees();
                }

                if (_employeeStore.Employees.Count > 0)
                {
                    await _bookingService.ExecuteAsync();
                }                

                await Task.Delay(60000, stoppingToken);
            }
        }

        private void DisplayRegisteredEmployees()
        {
            Console.WriteLine($"Registered employees: {_employeeStore.Employees.Count}\n");
            foreach (Employee employee in _employeeStore.Employees)
            {
                Console.WriteLine($"Id: {employee.Id,-10} | Name: {employee.Name,-20} | Start work: {employee.StartWork,-8} " +
                    $"| End work: {employee.EndWork,-8} | bookingStart: {employee.BookingStartWork,-8} | BookingEnd: {employee.BookingEndWork,-8}");
            }
        }

        private void DisplayActuallyLoggedInEmployees()
        {
            Console.WriteLine("\nActually logged in:\n");
            foreach (Employee employee in _employeeStore.Employees)
            {
                if (employee.LoggedIn)
                {
                    Console.WriteLine($"Id: {employee.Id,-10}Name: {employee.Name,-20}logged in");
                }
            }
        }
    }
}
