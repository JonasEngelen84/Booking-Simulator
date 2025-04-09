using OBS.Calendar.Client.Api;
using OBS.Stamm.Client.Api;
using OBS_Booking.Services.Configuration;
using OBS_Booking_App.Models;
using OBS_Booking_App.Services.Configuration;
using System;
using System.Collections.Generic;

namespace OBS_Booking_App.Stores
{
    public class EmployeeStore
    {
        private readonly IPersonsApi _stammApi;
        private readonly IPersonCalendarApi _calenderApi;
        private readonly EmployeesApiConfiguration _apiConfig;
        private readonly EmployeesAppsettingsConfiguration _appsettingsConfig;

        private List<Employee> _employees = new();
        public List<Employee> Employees => _employees;

        public EmployeeStore(
            IPersonsApi stammApi,
            IPersonCalendarApi calenderApi,
            EmployeesApiConfiguration apiConfig,
            EmployeesAppsettingsConfiguration appsettingsConfig)
        {
            _stammApi = stammApi;
            _calenderApi = calenderApi;
            _apiConfig = apiConfig;
            _appsettingsConfig = appsettingsConfig;
        }

        public void UpdateEmployees()
        {
            Console.WriteLine($"Update employees: {DateTime.Now}");
            if (_stammApi != null && _calenderApi != null)
                _employees = _apiConfig.Employees;

            if (_employees.Count < 6)
            {
                var fallbackEmployees = _appsettingsConfig.Employees;

                foreach (var emp in fallbackEmployees)
                {
                    _employees.Add(emp);
                }
            }
        }

        public void RemoveEmployee(Employee employee)
        {
            _employees.Remove(employee);
        }
    }
}
