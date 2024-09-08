using FluentAssertions;
using RestSharp;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Betsson.OnlineWallets.ApiTests
{
    public class BetssonOnlineWalletsApiTests
    {
        private readonly RestClient _client;

        public BetssonOnlineWalletsApiTests()
        {
            // RestSharp client
            _client = new RestClient("http://localhost:8080"); 
        }

        [Fact]
        public async Task GetBalance_ShouldReturnOkStatus()
        {
            // Arrange
            var request = new RestRequest("/onlinewallet/balance", Method.Get);

            // Act
            var response = await _client.ExecuteAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Should().Contain("amount"); 
        }

        
    }
}
