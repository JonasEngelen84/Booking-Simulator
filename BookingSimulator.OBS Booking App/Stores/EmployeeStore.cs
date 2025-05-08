using OBS.Calendar.Client.Api;
using OBS.Stamm.Client.Api;
using OBS_Booking_App.Models;
using OBS_Booking_App.Services.API;
using OBS_Booking_App.Services.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OBS_Booking_App.Stores
{
    public class EmployeeStore
    {
        private List<Employee> _employees = new();
        public List<Employee> Employees => _employees;

        IEnumerable<IEmployeesProvider> _providers;

        public EmployeeStore(IEnumerable<IEmployeesProvider> providers)
        {
            _providers = providers;
        }

        public void UpdateEmployees()
        {
            var ObsApiProvider = _providers.OfType<EmployeesApiConfiguration>().FirstOrDefault();
            var appsettingsProvider = _providers.OfType<EmployeesAppsettingsConfiguration>().FirstOrDefault();

            var apiEmployees = ObsApiProvider.Employees;
            if (apiEmployees != null && apiEmployees.Count >= 25)
            {
                _employees = new List<Employee>(apiEmployees);
            }
            else
            {
                Console.WriteLine("OBS.API.Configuration failed!\nUsing appsettings.Configuration");
                _employees = new List<Employee>(appsettingsProvider.Employees);
            }
        }

        public void RemoveEmployee(Employee employee)
        {
            _employees.Remove(employee);
        }
    }
}
