using System;

namespace OBS_Booking.Services.Configuration
{
    public class EmployeeConfiguration
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime StartContract { get; set; }
        public DateTime EndContract { get; set; }
        public DateTime StartWork { get; set; }
        public DateTime EndWork { get; set; }
    }
}
