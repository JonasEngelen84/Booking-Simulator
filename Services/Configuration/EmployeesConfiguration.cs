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
    public class EmployeesConfiguration : IEmployeesProvider
    {
        private readonly IPersonsApi _stammApi;
        private readonly IPersonCalendarApi _calenderApi;
        private readonly IBookingApi _bookingApi;
        private readonly ILogger _logger;
        Random rndObj = new();
        private List<Employee> EmployeesCache = new();

        public EmployeesConfiguration(
            IPersonsApi stammApi,
            IPersonCalendarApi calenderApi,
            IBookingApi bookingApi,
            ILogger logger)
        {
            _stammApi = stammApi;
            _calenderApi = calenderApi;
            _bookingApi = bookingApi;
            _logger = logger;
        }

        public List<Employee> Employees
        {
            get
            {
                string id = null;
                string name = null;
                DateTime? startContract = null;
                DateTime? endContract = null;
                DateTime? startWork = null;
                DateTime? endWork = null;
                DateTime? dateOfWork = null;

                foreach (var emp in _stammApi.All())
                {
                    try
                    {
                        id = emp.Id;
                        name = emp.Name;
                        startContract = emp.DateOfEntry;
                        endContract = emp.DateOfLeaving;

                        //TODO: CalendarDetailsCunfiguration verbessern
                        foreach (var employeeCalendarDetails in _calenderApi.GetSimpleFromNumberAndDateAsync(id, DateTime.Now.Date.ToUniversalTime()))
                        { Console.WriteLine(employeeCalendarDetails);
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
                            EmployeesCache.Add(new Employee(
                                id,
                                name,
                                startContract,
                                endContract,
                                startWork,
                                endWork,
                                dateOfWork));
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogInformation($"\n\nEmployee configuration is failed\nEmployeeId: {id} - Name: {name}", ex);
                        Console.WriteLine($"\n\nEmployee configuration is failed\nEmployeeId: {id} - Name: {name}" + ex);

                        continue;
                    }
                }
                return EmployeesCache;
            }
        }
    }
}
