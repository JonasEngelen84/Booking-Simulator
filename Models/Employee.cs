using System;

namespace OBS_Booking_App.Models
{
    public class Employee
    {
        public string Id { get; }
        public string Name { get; }
        public DateTime? StartContract { get; }
        public DateTime? EndContract { get; }
        public DateTime? StartWork { get; }
        public DateTime? EndWork { get; }
        public DateTime? DateOfWork { get; } = DateTime.Now;

        public bool LoggedIn = false;

        public Employee(
            string id,
            string name,
            DateTime? startContract,
            DateTime? endContract,
            DateTime? startWork,
            DateTime? endWork)
        {
            Id = id;
            Name = name;
            StartContract = startContract;
            EndContract = endContract;
            StartWork = startWork;
            EndWork = endWork;
            //StartWork = DateTime.Parse("13:20:00");       // Schichtbeginn manuell für alle Mitarbeiter festlegen.
            //EndWork = DateTime.Parse("13:23:00");         // Schichtende manuell für alle Mitarbeiter festlegen.
        }
    }
}
