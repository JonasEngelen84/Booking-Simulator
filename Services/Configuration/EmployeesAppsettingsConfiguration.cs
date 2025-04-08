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
                        var startContract = DateTime.Today.AddMonths(-rnd.Next(1, 12));
                        var endContract = DateTime.Today.AddMonths(rnd.Next(1, 12));

                        var baseStart = DateTime.Today.AddHours(8);
                        var baseEnd = DateTime.Today.AddHours(16);

                        int startOffset = rnd.Next(1, 10) <= 2 ? rnd.Next(0, 10) : rnd.Next(-10, 0);
                        int endOffset = rnd.Next(0, 10);

                        var startWork = baseStart.AddMinutes(startOffset);
                        var endWork = baseEnd.AddMinutes(endOffset);

                        employeesCache.Add(new Employee(
                            config.Id,
                            config.Name,
                            startContract,
                            endContract,
                            startWork,
                            endWork,
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
