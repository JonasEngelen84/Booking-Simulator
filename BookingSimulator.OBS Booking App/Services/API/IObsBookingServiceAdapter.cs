using OBS_Booking_App.Models;
using System.Threading.Tasks;

namespace OBS_Booking_App.Services.API
{
    public interface IObsBookingServiceAdapter
    {
        Task CreateBookingAsync(Employee employee, BookingAction action);
    }

    public enum BookingAction
    {
        Arrive,
        Leave
    }
}
