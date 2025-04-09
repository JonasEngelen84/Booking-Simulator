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
        private string _id { get; set; } = null;
        private string _name { get; set; } = null;
        private DateTime? _startContract { get; set; }
        private DateTime? _endContract { get; set; }
        private DateTime? _startWork { get; set; }
        private DateTime? _endWork { get; set; }
        private DateTime _bookingStartWork { get; set; }
        private DateTime _bookingEndWork { get; set; }
        private DateTime _dateOfWork { get; set; }

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
                    if (string.IsNullOrWhiteSpace(_id) ||
                        string.IsNullOrWhiteSpace(_name) ||
                        _startContract == null ||
                        _endContract == null)
                    {
                        _logger.LogInformation($"\n\nEmployee configuration is failed\nEmployeeId: {_id} - Name: {_name}");
                        Console.WriteLine($"\n\nEmployee configuration is failed\nEmployeeId: {_id} - Name: {_name}");
                        continue;
                    }

                    _id = emp.Id;
                    _name = emp.Name;
                    _startContract = emp.DateOfEntry;
                    _endContract = emp.DateOfLeaving;

                    //TODO: CalendarDetailsCunfiguration verbessern
                    foreach (var employeeCalendarDetails in _calenderApi.GetSimpleFromNumberAndDateAsync(_id, DateTime.Now.Date.ToUniversalTime()))
                    {
                        if (_startWork == null || _endWork == null || _dateOfWork == null)
                        {
                            _logger.LogInformation($"\n\nEmployee configuration is failed\nEmployeeId: {_id} - Name: {_name}");
                            Console.WriteLine($"\n\nEmployee configuration is failed\nEmployeeId: {_id} - Name: {_name}");
                            continue;
                        }

                        _dateOfWork = employeeCalendarDetails.Date;


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
                        DateTime parseTime = (DateTime)_startWork;
                        _bookingStartWork = parseTime.Add(TimeSpanStartWork);

                        // Schichtende bis zu 10 Minuten nach offiziellem Schichtende buchen.
                        rnd = rndObj.Next(0, 10);
                        TimeSpan TimeSpanEndWork = new TimeSpan(0, rnd, 0);
                        parseTime = (DateTime)_endWork;
                        _bookingEndWork = parseTime.Add(TimeSpanEndWork);
                    }

                    // Wenn Mitarbeiter heute berechtigt vertraglich zu arbeiten & schichtende noch nicht verstrichen ist
                    if (_dateOfWork == DateTime.Now.Date && _endWork > DateTime.Now && _startContract <= DateTime.Now.Date && _endContract > DateTime.Now.Date)
                    {
                        employeesCache.Add(new Employee(
                            _id,
                            _name,
                            _startContract,
                            _endContract,
                            _startWork,
                            _endWork,
                            _bookingStartWork,
                            _bookingEndWork,
                            _dateOfWork));
                    }
                }
                return employeesCache;
            }
        }
    }
}
