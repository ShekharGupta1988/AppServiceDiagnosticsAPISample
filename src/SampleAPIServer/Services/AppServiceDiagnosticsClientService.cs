using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SampleAPIServer.Interfaces;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SampleAPIServer.Services
{
    public class AppServiceDiagnosticsClientService : IAppServiceDiagnosticsClientService
    {
        private HttpClient httpClient;
        private ITokenService tokenService;
        private string authenticationMode;
        private string apiEndpoint;
        private string msiClientId;
        private string msiResourceId;
        
        public AppServiceDiagnosticsClientService(ITokenService tokenService, IConfiguration config)
        {
            this.tokenService = tokenService;
            apiEndpoint = config["DiagnosticServer:ApiEndpoint"].ToString();
            msiClientId = config["DiagnosticServer:MSIClientId"].ToString();
            msiResourceId = config["DiagnosticServer:MSIResourceId"].ToString();                   
            HttpClientHandler handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback +=
                (sender, cert, chain, sslPolicyErrors) => true;

            this.httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(60)
            };
        }
        
        public async Task<HttpResponseMessage> Execute(string resourceUrl, string region, IHeaderDictionary requestHeaders)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, apiEndpoint);
            string authToken = await this.tokenService.GetAuthorizationTokenAsync();
            requestMessage.Headers.Add("Authorization", authToken);
            requestMessage.Headers.Add("x-ms-path-query", resourceUrl);
            requestMessage.Headers.Add("x-ms-verb", "POST");
            AddAdditionalRequestHeaders(requestHeaders, ref requestMessage);
            return await this.httpClient.SendAsync(requestMessage);
        }

        private void AddAdditionalRequestHeaders(IHeaderDictionary incomingRequestHeaders, ref HttpRequestMessage request)
        {
            foreach (var header in incomingRequestHeaders)
            {
                if (header.Key.StartsWith("x-ms", StringComparison.OrdinalIgnoreCase) && !request.Headers.Contains(header.Key))
                {
                    request.Headers.Add(header.Key, header.Value.FirstOrDefault());
                }
            }
        }
    }
}
