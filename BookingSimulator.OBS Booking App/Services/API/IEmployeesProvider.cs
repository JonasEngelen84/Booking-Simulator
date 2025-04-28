using OBS_Booking_App.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBS_Booking_App.Services.API
{
    public interface IEmployeesProvider
    {
        List<Employee> Employees { get; }
    }
}
