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
            Console.WriteLine($"Update employees: {DateTime.Now}");

            var employeesApiProvider = _providers.OfType<EmployeesApiConfiguration>().FirstOrDefault();
            var appsettingsProvider = _providers.OfType<EmployeesAppsettingsConfiguration>().FirstOrDefault();

            if (employeesApiProvider != null && employeesApiProvider.Employees.Count >= 25)
            {
                _employees = new List<Employee>(employeesApiProvider.Employees);
            }
            else
            {
                Console.WriteLine("\nUsing appsettings.Configuration");
                _employees = new List<Employee>(appsettingsProvider.Employees);
            }
        }

        public void RemoveEmployee(Employee employee)
        {
            _employees.Remove(employee);
        }
    }
}
