using OBS.Booking.Client.Api;
using OBS.Booking.Client.Model;
using OBS_Booking.App.Services.API;
using System;
using System.Threading.Tasks;

namespace OBS_Booking.App.Services
{
    /// <summary>
    /// Adapterklasse zur Integration des OBS-Buchungsdienstes.
    /// 
    /// Diese Klasse kapselt die direkte Kommunikation mit der externen IBookingApi 
    /// und setzt die Buchung auf Basis der Domänenlogik (z. B. Kommen/Gehen) um.
    /// Sie implementiert die Schnittstelle <see cref="IObsBookingAdapter"/> 
    /// zur Entkopplung und besseren Testbarkeit.
    /// </summary>
    public class ObsBookingAdapterService : IObsBookingAdapter
    {
        private readonly IBookingApi _bookingApi;

        public ObsBookingAdapterService(IBookingApi bookingApi)
        {
            _bookingApi = bookingApi;
        }

        /// <summary>
        /// Führt eine Buchung (Kommen oder Gehen) für den angegebenen Mitarbeiter aus.
        /// 
        /// Die Buchung wird sofort mit dem aktuellen Zeitstempel registriert.
        /// </summary>
        /// <returns> Ein Task, der die asynchrone Buchungsoperation repräsentiert. </returns>
        public async Task CreateBookingAsync(string id, BookingAction action)
        {
            // Ermitteln des Buchungstyps auf Basis der Aktion (Kommen oder Gehen)
            BookingType bookingType = action == BookingAction.Arrive ? BookingType.ARRIVE : BookingType.LEAVE;

            // Explizites Mapping zwischen API-Model und Client-Model
            var clientBookingType = bookingType == BookingType.ARRIVE
                ? OBS.Booking.Client.Model.BookingType.ARRIVE
                : OBS.Booking.Client.Model.BookingType.LEAVE;

            // Erstellen des Buchungsmodells zur Übergabe an die externe API
            var bookingModel = new CreateBookingModel(
                reservedBookingId: 0,
                type: clientBookingType,
                momentOfCreation: DateTime.Now,
                dateOfValidity: default,
                personnelNumber: id,
                optionalData: null
            );

            // Senden der Buchung an das externe System
            await _bookingApi.CreateAsync(bookingModel);
        }
    }
}
