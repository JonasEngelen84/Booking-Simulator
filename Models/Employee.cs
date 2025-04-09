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
        public DateTime BookingStartWork { get; }
        public DateTime BookingEndWork { get; }

        public bool LoggedIn { get; set; }

        public Employee(
            string id,
            string name,
            DateTime? startContract,
            DateTime? endContract,
            DateTime? startWork,
            DateTime? endWork,
            DateTime bookingStartWork,
            DateTime bookingEndWork)
        {
            Id = id;
            Name = name;
            StartContract = startContract;
            EndContract = endContract;
            StartWork = startWork;
            EndWork = endWork;
            BookingStartWork = bookingStartWork;
            BookingEndWork = bookingEndWork;
        }
    }
}
