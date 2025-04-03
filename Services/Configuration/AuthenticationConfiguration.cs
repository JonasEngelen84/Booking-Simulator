using System.Collections.Generic;

namespace OBS_Booking_App.Services.Configuration
{
    class AuthenticationConfiguration
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public List<string> Scopes { get; set; }
    }
}
