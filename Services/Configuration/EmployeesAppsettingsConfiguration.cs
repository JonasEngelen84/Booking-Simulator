using Microsoft.Extensions.Configuration;
using OBS_Booking.Services.Configuration;
using OBS_Booking_App.Models;
using OBS_Booking_App.Services.API;
using System;
using System.Collections.Generic;

namespace OBS_Booking_App.Services.Configuration
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
                        int startOffset = rnd.Next(1, 10) <= 2 ? rnd.Next(0, 15) : rnd.Next(-15, 0);
                        int endOffset = rnd.Next(0, 15);

                        var bookingStartWork = DateTime.Now;
                        var bookingEndWork = DateTime.Now;

                        employeesCache.Add(new Employee(
                            config.Id,
                            config.Name,
                            config.StartContract,
                            config.EndContract,
                            config.StartWork,
                            config.EndWork,
                            bookingStartWork,
                            bookingEndWork)
                        {
                            LoggedIn = bookingStartWork <= DateTime.Now && bookingEndWork >= DateTime.Now
                        });
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
