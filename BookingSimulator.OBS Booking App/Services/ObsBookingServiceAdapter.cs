using OBS.Booking.Client.Api;
using OBS.Booking.Client.Model;
using OBS_Booking_App.Models;
using OBS_Booking_App.Services.API;
using System;
using System.Threading.Tasks;
using BookingType = OBS.Booking.ApiModel.Util.BookingType;

namespace OBS_Booking_App.Services
{
    public class ObsBookingServiceAdapter : IObsBookingServiceAdapter
    {
        private readonly IBookingApi _bookingApi;

        public ObsBookingServiceAdapter(IBookingApi bookingApi)
        {
            _bookingApi = bookingApi;
        }

        public async Task CreateBookingAsync(Employee employee, BookingAction action)
        {
            var bookingType = action == BookingAction.Arrive ? BookingType.ARRIVE : BookingType.LEAVE;
            var bookingModel = new CreateBookingModel(0, (OBS.Booking.Client.Model.BookingType?)bookingType, DateTime.Now, default, employee.Id, null);

            await _bookingApi.CreateAsync(bookingModel);
        }
    }
}
