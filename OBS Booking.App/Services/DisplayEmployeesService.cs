using OBS_Booking.App.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OBS_Booking.App.Services
{
    /// <summary>
    /// Service zur Ausgabe der Mitarbeiter
    /// </summary>
    public static class EmployeeDisplayService
    {
        public static void DisplayRegisteredEmployees(IEnumerable<Employee> employees)
        {
            Console.WriteLine($"\nRegistered employees: {employees.Count()}");

            foreach (var employee in employees)
            {
                Console.WriteLine($"| Id: {employee.Id,-7} | Name: {employee.Name,-25} | Start work: {employee.StartWork,-22} | End work: {employee.EndWork} |");
            }
        }

        public static void DisplayLoggedInEmployees(IEnumerable<Employee> employees)
        {
            var loggedInEmployees = employees.Where(e => e.LoggedIn).ToList();

            Console.WriteLine($"\n{DateTime.Now}    Logged in: {loggedInEmployees.Count}");

            foreach (var employee in loggedInEmployees)
            {
                Console.WriteLine($"Id: {employee.Id,-10}Name: {employee.Name}");
            }
        }
    }
}
