using Microsoft.Extensions.Logging;
using OBS.Calendar.Client.Api;
using OBS.Stamm.Client.Api;
using OBS_Booking_App.Models;
using OBS_Booking_App.Services.API;
using System;
using System.Collections.Generic;

namespace OBS_Booking_App.Services.Configuration
{
    public class EmployeesApiConfiguration : IEmployeesProvider
    {
        private string _id { get; set; }
        private string _name { get; set; }
        private DateTime? _startContract { get; set; }
        private DateTime? _endContract { get; set; }
        private DateTime? _startWork { get; set; }
        private DateTime? _endWork { get; set; }
        private DateTime _bookingStartWork { get; set; }
        private DateTime _bookingEndWork { get; set; }
        private bool _loggedIn { get; set; }

        private readonly IPersonsApi _stammApi;
        private readonly IPersonCalendarApi _calendarApi;
        private readonly ILogger<EmployeesApiConfiguration> _logger;

        Random rnd = new();
        private List<Employee> employeesCache = new();

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
                    if (string.IsNullOrWhiteSpace(emp.Id) ||
                        string.IsNullOrWhiteSpace(emp.Name) ||
                        emp.DateOfEntry == null ||
                        emp.DateOfLeaving == null)
                    {
                        _logger.LogInformation($"\n\nEmployee configuration is failed\nEmployeeId: {_id} - Name: {_name}");
                        Console.WriteLine($"\n\nEmployee configuration is failed\nEmployeeId: {_id} - Name: {_name}");
                        continue;
                    }

                    if (_startContract <= DateTime.Now.Date || _endContract > DateTime.Now.Date)
                        continue;

                    _id = emp.Id;
                    _name = emp.Name;
                    _startContract = emp.DateOfEntry;
                    _endContract = emp.DateOfLeaving;

                    foreach (var employeeCalendarDetails in _calendarApi.GetSimpleFromNumberAndDateAsync(_id, DateTime.Now.Date.ToUniversalTime()))
                    {
                        if (employeeCalendarDetails.EndTime == null || employeeCalendarDetails.StartTime == null)
                        {
                            _logger.LogInformation($"\n\nEmployee configuration is failed\nEmployeeId: {_id} - Name: {_name}");
                            Console.WriteLine($"\n\nEmployee configuration is failed\nEmployeeId: {_id} - Name: {_name}");
                            continue;
                        }

                        if (employeeCalendarDetails.Date != DateTime.Now.Date || _endWork > DateTime.Now)
                            continue ;

                        _startWork = employeeCalendarDetails.StartTime;
                        _endWork = employeeCalendarDetails.EndTime;

                        var baseStart = new DateTime(2025, 4, 9, 13, 30, 0);
                        var baseEnd = new DateTime(2025, 4, 9, 13, 45, 0);

                        int startOffset = rnd.Next(1, 10) <= 2 ? rnd.Next(0, 15) : rnd.Next(-15, 0);
                        int endOffset = rnd.Next(0, 15);

                        employeesCache.Add(new Employee(
                                _id,
                                _name,
                                _startContract,
                                _endContract,
                                _startWork = employeeCalendarDetails.StartTime,
                                _endWork = employeeCalendarDetails.EndTime,
                                _bookingStartWork = baseStart.AddMinutes(startOffset),
                                _bookingEndWork = baseEnd.AddMinutes(endOffset))
                        {
                            LoggedIn = _bookingStartWork <= DateTime.Now && _bookingEndWork >= DateTime.Now
                        });
                    }
                }
                return employeesCache;
            }
        }
    }
}
