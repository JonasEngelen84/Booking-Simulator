using Microsoft.Extensions.Logging;
using OBS.Calendar.Client.Api;
using OBS.Calendar.Client.Model;
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
                if (_stammApi == null || _calendarApi == null)
                    return employeesCache;

                foreach (var emp in _stammApi.All())
                {
                    try
                    {
                        ValidateEmployeeData(emp);

                        var calendarEntries = _calendarApi.GetSimpleFromNumberAndDateAsync(emp.Id, DateTime.Now.Date.ToUniversalTime());
                        DateTime? startTime = null;
                        DateTime? endTime = null;
                        foreach (var entry in calendarEntries)
                        {
                            ValidateCalendarEntry(entry);
                            startTime = entry.StartTime;
                            endTime = entry.EndTime;
                        }

                        CreateEmployee(emp.Id, emp.Name, startTime, endTime);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Employee data failure: {ex.Message}");
                    }
                }

                return employeesCache;
            }
        }

        private void ValidateEmployeeData(SimplePersonApiModel emp)
        {
            if (string.IsNullOrWhiteSpace(emp.Id))
                throw new ArgumentException("Employee ID is invalid.");

            if (string.IsNullOrWhiteSpace(emp.Name))
                throw new ArgumentException("Employee name is invalid.");

            //if (emp.DateOfEntry == null || emp.DateOfEntry <= DateTime.Now.Date)
            //    throw new ArgumentException($"Invalid entry date for employee: {emp.Name}");

            //if (emp.DateOfLeaving == null || emp.DateOfLeaving > DateTime.Now.Date)
            //    throw new ArgumentException($"Invalid leaving date for employee: {emp.Name}");
        }

        private void ValidateCalendarEntry(SimplePersonCalendarApiModel entry)
        {
            if (entry.StartTime == null || entry.EndTime == null)
                throw new ArgumentException($"Invalid StartTime or EndTime for employee: {entry.PersonId}");

            //if (entry.Date.Date != DateTime.Now.Date && entry.Date.Date != DateTime.Now.Date.AddDays(-1))
            //    throw new ArgumentException($"Kalendereintrag ist weder für heute ({today}) noch gestern ({yesterday}).");
        }

        private void CreateEmployee(string id, string name, DateTime? startWork, DateTime? endWork)
        {
            var (bookingStart, bookingEnd) = GenerateBookingTimes(startWork, endWork);

            employeesCache.Add(new Employee(
                    id,
                    name,
                    startWork,
                    endWork,
                    bookingStart,
                    bookingEnd)
            {
                LoggedIn = bookingStart <= DateTime.Now && bookingEnd >= DateTime.Now
            });
        }

        private (DateTime bookingStart, DateTime bookingEnd) GenerateBookingTimes(DateTime? start, DateTime? end)
        {
            var rnd = new Random();

            int startOffset = rnd.Next(1, 10) == 1 ? rnd.Next(0, 10) : rnd.Next(-10, 0);
            int endOffset = rnd.Next(1, 10) <= 3 ? rnd.Next(0, 10) : rnd.Next(-10, 0);

            return (start.Value.AddMinutes(startOffset), end.Value.AddMinutes(endOffset));
        }
    }
}
