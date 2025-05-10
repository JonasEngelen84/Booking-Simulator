using OBS_Booking_App.Models;
using OBS_Booking_App.Services.API;
using OBS_Booking_App.Services.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OBS_Booking_App.Stores
{
    /// <summary>
    /// Verwaltet den zentralen employee-Zwischenspeicher für die Anwendung.
    /// Die employees werden zur Laufzeit von verschiedenen Datenquellen geladen.
    /// Bevorzugt wird der API-Provider (EmployeesApiConfiguration), sofern mindestens 25 gültige Datensätze vorhanden sind.
    /// Fallback ist der Appsettings-Provider (EmployeesAppsettingsConfiguration).
    /// </summary>
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
            // Implementierende Klassen der IEmployeesProvider bereitstellen
            var obsApiProvider = _providers.OfType<EmployeesApiConfiguration>().FirstOrDefault();
            var appsettingsProvider = _providers.OfType<EmployeesAppsettingsConfiguration>().FirstOrDefault();

            if (obsApiProvider == null || appsettingsProvider == null)
            {
                Console.WriteLine("EmployeeStore: Configuration failed – all provider has been registered.");
                throw new InvalidOperationException("EmployeeStore: Employee-provider is missing.");
            }

            // Employees aus EmployeesApiConfiguration laden
            var apiEmployees = obsApiProvider.Employees;
            if (apiEmployees.Count >= 25)
            {
                _employees = new List<Employee>(apiEmployees);
            }
            else
            {
                Console.WriteLine("EmployeeStore: OBS.API.Configuration failed!\nUsing appsettings.Configuration");
                _employees = new List<Employee>(appsettingsProvider.Employees);
            }
        }

        public void RemoveEmployee(Employee employee)
        {
            _employees.Remove(employee);
        }
    }
}
