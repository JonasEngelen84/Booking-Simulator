using OBS_Booking_App.Models;
using System.Threading.Tasks;

namespace OBS_Booking_App.Services.API
{
    /// <summary>
    /// Definiert eine Schnittstelle zur Durchführung von Buchungsaktionen (Kommen/Gehen) 
    /// über ein externes OBS-Buchungssystem.
    /// 
    /// Diese Abstraktion dient dazu, die konkrete Implementierung der Buchungslogik zu entkoppeln.
    /// Dadurch wird eine bessere Testbarkeit (z. B. durch Mocking) und Erweiterbarkeit erreicht.
    /// </summary>
    public interface IObsBookingAdapter
    {
        /// <summary>
        /// Erstellt eine Buchung für den angegebenen Mitarbeiter mit der spezifizierten Aktion (z. B. Kommen oder Gehen).
        /// </summary>
        /// <returns> Ein Task, der die asynchrone Operation repräsentiert. </returns>
        Task CreateBookingAsync(string id, BookingAction action);
    }

    // Enum zur Repräsentation von Buchungsaktionen
    public enum BookingAction
    {
        Arrive,
        Leave
    }
}
