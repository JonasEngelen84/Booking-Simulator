namespace OBS_Booking.App.Services.Configuration
{
    /// <summary>
    /// Repräsentiert die Service-Endpunkte der OBS-Plattform,
    /// welche für den Zugriff auf verschiedene APIs benötigt werden.
    /// Diese Konfiguration wird in AuthenticationService sowie in der Initialisierung
    /// von IPersonsApi, IPersonCalendarApi und IBookingApi verwendet.
    /// </summary>
    class ObsServicesConfiguration
    {
        // URL des STS (Security Token Service) zur Authentifizierung und Token-Generierung.
        public string STSServiceUrl { get; set; }
        public string StammServiceUrl { get; set; }
        public string CalendarServiceUrl { get; set; }
        public string BookingServiceUrl { get; set; }
    }
}
