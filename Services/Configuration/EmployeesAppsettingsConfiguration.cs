using Microsoft.Extensions.Configuration;
using OBS_Booking_App.Models;
using OBS_Booking_App.Services.API;
using System;
using System.Collections.Generic;

namespace OBS_Booking.Services.Configuration
{
    public class EmployeesAppsettingsConfiguration : IEmployeesProvider
    {
        private readonly IConfiguration _configuration;

        public EmployeesAppsettingsConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
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
                        var baseStart = new DateTime(2025, 4, 9, 13, 30, 0);
                        var baseEnd = new DateTime(2025, 4, 9, 13, 45, 0);

                        int startOffset = rnd.Next(1, 10) <= 2 ? rnd.Next(0, 10) : rnd.Next(-10, 0);
                        int endOffset = rnd.Next(0, 10);

                        var bookingStartWork = baseStart.AddMinutes(startOffset);
                        var bookingEndWork = baseEnd.AddMinutes(endOffset);

                        employeesCache.Add(new Employee(
                            config.Id,
                            config.Name,
                            config.StartContract,
                            config.EndContract,
                            config.StartWork,
                            config.EndWork,
                            bookingStartWork,
                            bookingEndWork,
                            DateTime.Today));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
                return employeesCache;
            }
        }
    }
}
