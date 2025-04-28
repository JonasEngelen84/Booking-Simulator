using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OBS_Booking.Services.Configuration;
using OBS_Booking_App.Models;
using OBS_Booking_App.Services.API;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Xml.Linq;

namespace OBS_Booking_App.Services.Configuration
{
    public class EmployeesAppsettingsConfiguration : IEmployeesProvider
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmployeesAppsettingsConfiguration> _logger;

        public EmployeesAppsettingsConfiguration(IConfiguration configuration, ILogger<EmployeesAppsettingsConfiguration> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public List<Employee> Employees
        {
            get
            {
                var configs = _configuration.GetSection("Employees").Get<List<EmployeeConfiguration>>();
                var employeesCache = new List<Employee>();
                var rnd = new Random();

                foreach (var config in configs)
                {
                    try
                    {
                        DateTime startWork = config.EndWork.AddHours(-8);

                        int startOffset = rnd.Next(1, 10) <= 2 ? rnd.Next(0, 15) : rnd.Next(-15, 0);
                        int endOffset = rnd.Next(0, 15);

                        var bookingStartWork = startWork.AddMinutes(startOffset);
                        var bookingEndWork = config.EndWork.AddMinutes(endOffset);

                        employeesCache.Add(new Employee(
                            config.Id,
                            config.Name,
                            startWork,
                            config.EndWork,
                            bookingStartWork,
                            bookingEndWork)
                        {
                            LoggedIn = bookingStartWork <= DateTime.Now && bookingEndWork >= DateTime.Now
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogInformation($"\n\nEmployee configuration is failed\nEmployeeId: {config.Id} - Name: {config.Name}\n {ex.ToString()}");
                        Console.WriteLine($"\n\nEmployee configuration is failed\nEmployeeId: {config.Id} - Name: {config.Name}\n {ex.ToString()}");
                    }
                }
                return employeesCache;
            }
        }
    }
}
