using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using OBS.Calender.ApiModel.Out.Simple;
using OBS.Calendar.Client.Api;
using OBS.Stamm.Client.Api;
using OBS.Booking.Client.Api;
using OBS_Booking_App.Models;
using OBS_Booking_App.Services;
using OBS_Booking_App.Services.Configuration;
using OBS_Booking_App.Services.API;

namespace OBS_Booking_App.Services.Configuration
{
    public class EmployeesApiConfiguration : IEmployeesProvider
    {
        private string id { get; set; } = null;
        private string name { get; set; } = null;
        private DateTime? startContract { get; set; } = null;
        private DateTime? endContract { get; set; } = null;
        private DateTime? startWork { get; set; } = null;
        private DateTime? endWork { get; set; } = null;
        private DateTime? dateOfWork { get; set; } = null;

        private readonly IPersonsApi _stammApi;
        private readonly IPersonCalendarApi _calenderApi;
        private readonly ILogger<EmployeesApiConfiguration> _logger;
        Random rndObj = new();
        private List<Employee> employeesCache = new();

        public EmployeesApiConfiguration(
            IPersonsApi stammApi,
            IPersonCalendarApi calenderApi,
            ILogger<EmployeesApiConfiguration> logger)
        {
            _stammApi = stammApi;
            _calenderApi = calenderApi;
            _logger = logger;
        }

        public List<Employee> Employees
        {
            get
            {
                foreach (var emp in _stammApi.All())
                {
                    if (string.IsNullOrWhiteSpace(id) ||
                        string.IsNullOrWhiteSpace(name) ||
                        startContract == null ||
                        endContract == null)
                    {
                        _logger.LogInformation($"\n\nEmployee configuration is failed\nEmployeeId: {id} - Name: {name}");
                        Console.WriteLine($"\n\nEmployee configuration is failed\nEmployeeId: {id} - Name: {name}");
                        continue;
                    }

                    id = emp.Id;
                    name = emp.Name;
                    startContract = emp.DateOfEntry;
                    endContract = emp.DateOfLeaving;

                    //TODO: CalendarDetailsCunfiguration verbessern
                    foreach (var employeeCalendarDetails in _calenderApi.GetSimpleFromNumberAndDateAsync(id, DateTime.Now.Date.ToUniversalTime()))
                    {
                        if (startWork == null || endWork == null || dateOfWork == null)
                        {
                            _logger.LogInformation($"\n\nEmployee configuration is failed\nEmployeeId: {id} - Name: {name}");
                            Console.WriteLine($"\n\nEmployee configuration is failed\nEmployeeId: {id} - Name: {name}");
                            continue;
                        }

                        dateOfWork = employeeCalendarDetails.Date;

                        // Bestimmen der Buchung bei Schichtbeginn.
                        // Bei einem random Ergebnis von 1, oder 2 (Spanne von 1 - 10).
                        int rnd = rndObj.Next(1, 10);
                        if (rnd <= 2)
                        {
                            // bis zu 10 Minuten NACH offiziellem Schichtbeginn buchen.
                            rnd = rndObj.Next(0, 10);
                        }
                        else
                        {
                            // sonst bis zu 10 Minuten VOR offiziellem Schichtbeginn buchen.
                            rnd = rndObj.Next(-10, 0);
                        }
                        TimeSpan TimeSpanStartWork = new TimeSpan(0, rnd, 0);
                        DateTime parseTime = (DateTime)startWork;
                        startWork = parseTime.Add(TimeSpanStartWork);

                        // Schichtende bis zu 10 Minuten nach offiziellem Schichtende buchen.
                        rnd = rndObj.Next(0, 10);
                        TimeSpan TimeSpanEndWork = new TimeSpan(0, rnd, 0);
                        parseTime = (DateTime)endWork;
                        endWork = parseTime.Add(TimeSpanEndWork);
                    }

                    // Wenn Mitarbeiter heute berechtigt vertraglich zu arbeiten & schichtende noch nicht verstrichen ist
                    if (dateOfWork == DateTime.Now.Date && endWork > DateTime.Now && startContract <= DateTime.Now.Date && endContract > DateTime.Now.Date)
                    {
                        employeesCache.Add(new Employee(
                            id,
                            name,
                            startContract,
                            endContract,
                            startWork,
                            endWork,
                            dateOfWork));
                    }
                }
                return employeesCache;
            }
        }
    }
}
