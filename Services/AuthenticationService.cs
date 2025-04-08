using IdentityModel.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OBS_Booking_App.Services.Configuration;
using System;
using System.Linq;
using System.Net.Http;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;

namespace OBS_Booking_App.Services
{
    class AuthenticationService
    {
        private readonly IOptions<AuthenticationConfiguration> _authConfig;
        private readonly IOptions<ServicesConfiguration> _servicesConfig;
        private readonly ILogger<AuthenticationService> _logger;

        public AuthenticationService(IOptions<AuthenticationConfiguration> authConfig, ILogger<AuthenticationService> logger, IOptions<ServicesConfiguration> servicesConfig)
        {
            _authConfig = authConfig;
            _logger = logger;
            _servicesConfig = servicesConfig;
        }

        public async Task<string> GetAccessTokenAsync(CancellationToken token)
        {   _logger.LogInformation("Starting request for authentication token from STS.");
            Console.WriteLine("Starting request for authentication token from STS");
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
