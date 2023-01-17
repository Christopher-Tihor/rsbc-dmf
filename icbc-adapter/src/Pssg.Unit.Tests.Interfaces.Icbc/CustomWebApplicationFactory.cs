using Grpc.Net.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pssg.Interfaces;
using Pssg.Interfaces.Icbc.Helpers;
using Rsbc.Dmf.CaseManagement.Service;
using System.Net;
using System.Net.Http;
using Rsbc.Dmf.CaseManagement.Helpers;

namespace Rsbc.Dmf.IcbcAdapter.Tests
{
    public class CustomWebApplicationFactory<TStartup>
        : WebApplicationFactory<Startup>
    {
        public IConfiguration Configuration;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            
            IIcbcClient icbcClient ;
            Configuration = new ConfigurationBuilder()
                    .AddUserSecrets<Startup>()
                    .AddEnvironmentVariables()
                    .Build();

            if (Configuration["ICBC_SERVICE_URI"] != null)
            {
                icbcClient = new IcbcClient(Configuration);
            }
            else
            {
                icbcClient = IcbcHelper.CreateMock();
            }

            string cmsAdapterURI = Configuration["CMS_ADAPTER_URI"];

            CaseManager.CaseManagerClient caseManagerClient;
            if (string.IsNullOrEmpty(cmsAdapterURI))
            {
                // setup from Mock
                caseManagerClient = CmsHelper.CreateMock(Configuration);
            }
            else
            {
                var httpClientHandler = new HttpClientHandler();
                // Return `true` to allow certificates that are untrusted/invalid                    
                httpClientHandler.ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

                var httpClient = new HttpClient(httpClientHandler);
                // set default request version to HTTP 2.  Note that Dotnet Core does not currently respect this setting for all requests.
                httpClient.DefaultRequestVersion = HttpVersion.Version20;

                if (!string.IsNullOrEmpty(Configuration["CMS_ADAPTER_JWT_SECRET"]))
                {
                    var initialChannel = GrpcChannel.ForAddress(cmsAdapterURI, new GrpcChannelOptions { HttpClient = httpClient });

                    var initialClient = new CaseManager.CaseManagerClient(initialChannel);
                    // call the token service to get a token.
                    var tokenRequest = new CaseManagement.Service.TokenRequest
                    {
                        Secret = Configuration["CMS_ADAPTER_JWT_SECRET"]
                    };

                    var tokenReply = initialClient.GetToken(tokenRequest);

                    if (tokenReply != null && tokenReply.ResultStatus == CaseManagement.Service.ResultStatus.Success)
                    {
                        // Add the bearer token to the client.
                        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {tokenReply.Token}");
                    }
                }

                var channel = GrpcChannel.ForAddress(cmsAdapterURI, new GrpcChannelOptions { HttpClient = httpClient });
                caseManagerClient = new CaseManager.CaseManagerClient(channel);
            }

            builder
                .UseSolutionRelativeContentRoot("")
                .UseEnvironment("Staging")
                .UseConfiguration(Configuration)
                //.UseStartup<Startup>()
                .ConfigureTestServices(
               
                    services => {                         
                        services.AddTransient(_ => caseManagerClient);
                        services.AddTransient(_ => icbcClient);
                    });

        }
    }

    
}
