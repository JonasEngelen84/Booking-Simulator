using System;

namespace OBS_Booking.App.Models
{
    public class Employee
    {
        public string Id { get; }
        public string Name { get; }
        public DateTime? StartWork { get; }
        public DateTime? EndWork { get; }
        public DateTime BookingStartWork { get; }
        public DateTime BookingEndWork { get; }

        public bool LoggedIn { get; set; }

        public Employee(
            string id,
            string name,
            DateTime? startWork,
            DateTime? endWork,
            DateTime bookingStartWork,
            DateTime bookingEndWork)
        {
            Id = id;
            Name = name;
            StartWork = startWork;
            EndWork = endWork;
            BookingStartWork = bookingStartWork;
            BookingEndWork = bookingEndWork;
        }
    }
}
