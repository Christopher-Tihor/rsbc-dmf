using Newtonsoft.Json;
using Rsbc.Dmf.DriverPortal.Api.Controllers;
using Rsbc.Dmf.DriverPortal.ViewModels;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Rsbc.Dmf.DriverPortal.Tests
{
    [Collection(nameof(HttpClientCollection))]
    public class CaseTests : ApiIntegrationTestBase
    {
        public CaseTests(HttpClientFixture fixture): base(fixture) { }

        private const string CASE_API = "/api/Cases";

        [Fact]
        public async Task GetCase()
        {
            var caseId = Configuration["ICBC_TEST_CASEID"];
            var request = new HttpRequestMessage(HttpMethod.Get, 
                $"{CASE_API}/{nameof(CasesController.GetCase)}/ " + caseId);

            var response = await _client.SendAsync(request);
            var jsonString = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();

            var clientResult = JsonConvert.DeserializeObject<CaseDetail>(jsonString);

            Assert.Equal(clientResult.CaseId, caseId);
        }

        [Fact]
        public async Task GetMostRecentCase()
        {
            var licenseNumber = Configuration["ICBC_TEST_DL"];
            var request = new HttpRequestMessage(HttpMethod.Get, 
                $"{CASE_API}/{nameof(CasesController.GetMostRecentCase)}/" + licenseNumber);

            var response = await _client.SendAsync(request);
            var jsonString = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();

            var clientResult = JsonConvert.DeserializeObject<CaseDetail>(jsonString);

            Assert.NotNull(clientResult);
        }
    }
}