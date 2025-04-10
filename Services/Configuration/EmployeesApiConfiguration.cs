using Microsoft.Extensions.Logging;
using OBS.Calendar.Client.Api;
using OBS.Stamm.Client.Api;
using OBS.Stamm.Client.Model;
using OBS_Booking_App.Models;
using OBS_Booking_App.Services.API;
using System;
using System.Collections.Generic;

namespace OBS_Booking_App.Services.Configuration
{
    public class EmployeesApiConfiguration : IEmployeesProvider
    {
        private string _id;
        private string _name;

        List<Employee> employeesCache = new();

        private readonly IPersonsApi _stammApi;
        private readonly IPersonCalendarApi _calendarApi;
        private readonly ILogger<EmployeesApiConfiguration> _logger;

        public EmployeesApiConfiguration(
            IPersonsApi stammApi,
            IPersonCalendarApi calenderApi,
            ILogger<EmployeesApiConfiguration> logger)
        {
            _stammApi = stammApi;
            _calendarApi = calenderApi;
            _logger = logger;
        }

        public List<Employee> Employees
        {
            get
            {
                foreach (var emp in _stammApi.All())
                {
                    try
                    {
                        if (CheckData(emp))
                            continue;

                        _id = emp.Id;
                        _name = emp.Name;

                        CreateEmployee();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogInformation($"\n\nEmployee configuration is failed\nEmployeeId: {_id} - Name: {_name}\n {ex.ToString()}");
                        Console.WriteLine($"\n\nEmployee configuration is failed\nEmployeeId: {_id} - Name: {_name}\n {ex.ToString()}");
                    }
                }
                return employeesCache;
            }
        }

        private bool CheckData(SimplePersonApiModel emp)
        {
            if (string.IsNullOrWhiteSpace(emp.Id) ||
                        string.IsNullOrWhiteSpace(emp.Name) ||
                        emp.DateOfEntry == null ||
                        emp.DateOfLeaving == null)
            {
                _logger.LogInformation($"\n\nEmployee configuration is failed\nEmployeeId: {_id} - Name: {_name}");
                Console.WriteLine($"\n\nEmployee configuration is failed\nEmployeeId: {_id} - Name: {_name}");
                return true;
            }
            else if (emp.DateOfEntry <= DateTime.Now.Date || emp.DateOfLeaving > DateTime.Now.Date)
                return true;
            else
                return false;
        }

        private void CreateEmployee()
        {
            foreach (var employeeCalendarDetails in _calendarApi.GetSimpleFromNumberAndDateAsync(_id, DateTime.Now.Date.ToUniversalTime()))
            {
                if (employeeCalendarDetails.EndTime == null || employeeCalendarDetails.StartTime == null)
                {
                    _logger.LogInformation($"\n\nEmployee configuration is failed\nEmployeeId: {_id} - Name: {_name}");
                    Console.WriteLine($"\n\nEmployee configuration is failed\nEmployeeId: {_id} - Name: {_name}");
                    continue;
                }

                if (employeeCalendarDetails.Date != DateTime.Now.Date || employeeCalendarDetails.Date != DateTime.Now.Date.AddDays(-1))
                    continue;

                var startWork = employeeCalendarDetails.StartTime;
                var endWork = employeeCalendarDetails.EndTime;

                Random rnd = new();
                int startOffset = rnd.Next(1, 10) <= 2 ? rnd.Next(0, 15) : rnd.Next(-15, 0);
                int endOffset = rnd.Next(0, 15);

                var bookingStartWork = startWork.Value.AddMinutes(startOffset);
                var bookingEndWork = endWork.Value.AddMinutes(endOffset);

                employeesCache.Add(new Employee(
                        _id,
                        _name,
                        startWork,
                        endWork,
                        bookingStartWork,
                        bookingEndWork)
                {
                    LoggedIn = bookingStartWork <= DateTime.Now && bookingEndWork >= DateTime.Now
                });
            }
        }
    }
}
