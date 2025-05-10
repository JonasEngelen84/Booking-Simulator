using System.Collections.Generic;

namespace OBS_Booking_App.Services.Configuration
{
    /// <summary>
    /// Konfigurationsdaten für die Authentifizierung mittels Client-Credentials über den STS-Server.
    /// Diese Einstellungen werden im AuthenticationService verwendet, um Zugriffstokens für API-Aufrufe zu erhalten.
    /// </summary>
    class ObsAuthenticationConfiguration
    {
        public string ClientId { get; set; }        // Die Client-ID zur Authentifizierung bei der OBS-Plattform
        public string ClientSecret { get; set; }    // Das zugehörige Client-Secret zur Authentifizierung
        public List<string> Scopes { get; set; }    // Die Scopes (Zugriffsrechte), die im Authentifizierungsprozess angefordert werden
    }
}
