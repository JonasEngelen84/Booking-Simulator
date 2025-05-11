using IdentityModel.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OBS_Booking.App.Services.Configuration;
using System.Linq;
using System.Net.Http;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;

namespace OBS_Booking.App.Services
{
    /// <summary>
    /// Der AuthenticationService ist verantwortlich für die Authentifizierung gegen den zentralen STS (Security Token Service) 
    /// mittels OAuth2 Client Credentials Flow. 
    /// Er stellt Zugriffstokens für den Zugriff auf geschützte OBS-APIs bereit.
    /// </summary>
    class AuthenticationService
    {
        private readonly IOptions<ObsAuthenticationConfiguration> _authConfig;
        private readonly IOptions<ObsServicesConfiguration> _servicesConfig;
        private readonly ILogger<AuthenticationService> _logger;

        public AuthenticationService(IOptions<ObsAuthenticationConfiguration> authConfig, ILogger<AuthenticationService> logger, IOptions<ObsServicesConfiguration> servicesConfig)
        {
            _authConfig = authConfig;
            _logger = logger;
            _servicesConfig = servicesConfig;
        }

        /// <summary>
        /// Fordert ein Access Token beim STS (Security Token Service) an.
        /// Verwendet OpenID Connect Discovery und den Client-Credentials-Flow.
        /// </summary>
        /// <param name="token">CancellationToken zur Abbruchkontrolle.</param>
        /// <returns> Ein gültiges Access Token als string.</returns>
        /// <exception cref="AuthenticationException"> Wird geworfen, wenn das Discovery-Dokument nicht abgerufen werden kann.</exception>
        /// <exception cref="InvalidCredentialException"> Wird geworfen, wenn die Tokenanfrage fehlschlägt.</exception>
        public async Task<string> GetAccessTokenAsync(CancellationToken token)
        {   _logger.LogInformation("Starting request for authentication token from STS.");

            // Discovery-Dokument vom STS laden (enthält z. B. Token-Endpoint)
            var obsIdentityUrl = _servicesConfig.Value.STSServiceUrl;
            _logger.LogInformation($"Contacting STS Server at: '{obsIdentityUrl}' for OpenID Configuration.");
            var client = new HttpClient();
            var disco = await client.GetDiscoveryDocumentAsync($"{obsIdentityUrl}", cancellationToken: token);

            if (disco.IsError)
            {
                _logger.LogInformation("Failed to retrieve the discovery document. Throwing failure back to caller.");
                throw new AuthenticationException(disco.Error);
            }

            _logger.LogInformation($"Discovery document found. Requesting credentials for Client: '{_authConfig.Value.ClientId}' with scopes: '{_authConfig.Value.Scopes.Aggregate((l, r) => $"{l}, {r}")}'");

            // Zugriffstoken über Client-Credentials anfordern
            var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,

                ClientId = _authConfig.Value.ClientId,
                ClientSecret = _authConfig.Value.ClientSecret,
                Scope = _authConfig.Value.Scopes.Aggregate((l, r) => $"{l} {r}")
            },
            cancellationToken: token);

            if (tokenResponse.IsError)
            {
                _logger.LogInformation("Failed to request credentials. An authentication error has occured. Throwing failure back to caller.");
                throw new InvalidCredentialException(tokenResponse.Error);
            }

            _logger.LogInformation($"Authentication token received. It expires in: {tokenResponse.ExpiresIn} seconds.");

            return tokenResponse.AccessToken;
        }
    }
}
