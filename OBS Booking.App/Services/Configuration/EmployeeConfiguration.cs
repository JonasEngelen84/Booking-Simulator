using System;

namespace OBS_Booking.App.Services.Configuration
{
    public class EmployeeConfiguration
    {
        /// <summary>
        /// Employee-Daten aus appsettings.json.
        /// Diese werden in EmployeesAppsettingsConfiguration verarbeitet.
        /// </summary>
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime StartWork { get; set; }
    }
}
