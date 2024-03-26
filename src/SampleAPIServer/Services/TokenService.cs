using Microsoft.Extensions.Configuration;
using SampleAPIServer.Interfaces;
using System;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Core;

namespace SampleAPIServer.Services
{
    public class TokenService : ITokenService
    {
        private string msiClientId;
        private string msiResourceId;
        private ValueTask<AccessToken> acquireTokenTask;
        private bool tokenAcquiredAtleastOnce;

        /// <summary>
        /// Gets AAD issued auth token.
        /// </summary>
        public string AuthorizationToken
        {
            get; private set;
        }

        public TokenService(IConfiguration configuration)
        {
            LoadConfigurations(configuration);
            GetAccessTokenAsync();
        }

        // TODO: Implement token refresh.
        public async Task GetAccessTokenAsync()
        {

            try
            {
                var tokenCredential = new ManagedIdentityCredential(msiClientId);
                acquireTokenTask = tokenCredential.GetTokenAsync(
                                new TokenRequestContext(scopes: new string[] { $"{msiResourceId}/.default" }) { });
                AccessToken token = await acquireTokenTask;
                AuthorizationToken = GetAuthTokenFromValueTask(token);
                tokenAcquiredAtleastOnce = true;
            }
            catch (Exception ex)
            {
                // TODO: Handle exception
            }
        }

        private void LoadConfigurations(IConfiguration config)
        {
            msiClientId = config["DiagnosticServer:MSIClientId"].ToString();
            msiResourceId = config["DiagnosticServer:MSIResourceId"].ToString();
        }
        public async Task<string> GetAuthorizationTokenAsync()
        {
            if (!tokenAcquiredAtleastOnce)
            {
                var authResult = await acquireTokenTask;
                return GetAuthTokenFromValueTask(authResult);
            }

            return AuthorizationToken;
        }

        private string GetAuthTokenFromValueTask(AccessToken accessToken)
        {
            return $"Bearer {accessToken.Token}";
        }
    }
}
