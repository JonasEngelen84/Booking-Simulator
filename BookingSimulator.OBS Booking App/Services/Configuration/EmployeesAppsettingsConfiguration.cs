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
                        DateTime endWork = config.StartWork.AddHours(+8);

                        int startOffset = rnd.Next(1, 10) == 1 ? rnd.Next(0, 10) : rnd.Next(-10, 0);
                        int endOffset = rnd.Next(1, 10) <= 3 ? rnd.Next(0, 10) : rnd.Next(-10, 0);

                        var bookingStartWork = config.StartWork.AddMinutes(startOffset);
                        var bookingEndWork = endWork.AddMinutes(endOffset);

                        employeesCache.Add(new Employee(
                            config.Id,
                            config.Name,
                            config.StartWork,
                            endWork,
                            bookingStartWork,
                            bookingEndWork)
                        {
                            LoggedIn = bookingStartWork <= DateTime.Now && bookingEndWork >= DateTime.Now
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogInformation($"\n\nEmployee configuration is failed\nEmployeeId: {config.Id} Name: {config.Name}\n {ex.ToString()}");
                        Console.WriteLine($"\n\nEmployee configuration is failed\nEmployeeId: {config.Id} Name: {config.Name}\n {ex.ToString()}");
                    }
                }
                return employeesCache;
            }
        }
    }
}
