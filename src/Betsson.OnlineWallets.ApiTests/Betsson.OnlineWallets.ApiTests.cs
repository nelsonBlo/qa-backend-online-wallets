using FluentAssertions;
using RestSharp;
using System.Net;
using System.Text.Json;
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
            var request = new RestRequest("/onlinewallet/balance", Method.Get);

            var response = await _client.ExecuteAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Should().Contain("amount"); 
        }
        private async Task<decimal> GetCurrentBalanceAsync()
        {
            var request = new RestRequest("/onlinewallet/balance", Method.Get);
            var response = await _client.ExecuteAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseBody = JsonSerializer.Deserialize<BalanceResponse>(response.Content);
            return responseBody.amount;
        }

        [Fact]
        public async Task DepositFunds_ShouldIncreaseBalance()
        {
            var initialBalance = await GetCurrentBalanceAsync();

            var depositAmount = 100;
            var depositRequest = new RestRequest("/onlinewallet/deposit", Method.Post);
            depositRequest.AddJsonBody(new { amount = depositAmount });

            var depositResponse = await _client.ExecuteAsync(depositRequest);
            depositResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var newBalance = await GetCurrentBalanceAsync();
            newBalance.Should().Be(initialBalance + depositAmount);
        }

        [Fact]
        public async Task WithdrawFunds_ShouldDecreaseBalance()
        {
            var initialBalance = await GetCurrentBalanceAsync();

            var withdrawAmount = 10;
            var withdrawRequest = new RestRequest("/onlinewallet/withdraw", Method.Post);
            withdrawRequest.AddJsonBody(new { amount = withdrawAmount });

            var withdrawResponse = await _client.ExecuteAsync(withdrawRequest);
            withdrawResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var newBalance = await GetCurrentBalanceAsync();
            newBalance.Should().Be(initialBalance - withdrawAmount);
        }

        [Fact]
        public async Task WithdrawFunds_ShouldFailWithInsufficientBalance()
        {
            var initialBalance = await GetCurrentBalanceAsync();

            var withdrawAmount = initialBalance + 1; 
            var withdrawRequest = new RestRequest("/onlinewallet/withdraw", Method.Post);
            withdrawRequest.AddJsonBody(new { amount = withdrawAmount });

            var withdrawResponse = await _client.ExecuteAsync(withdrawRequest);

            withdrawResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest); 
        }
        
        [Fact]
        public async Task DepositFunds_ShouldHandleNegativeAmount()
        {
            var initialBalance = await GetCurrentBalanceAsync();

            var depositAmount = -500; 
            var depositRequest = new RestRequest("/onlinewallet/deposit", Method.Post);
            depositRequest.AddJsonBody(new { Amount = depositAmount });

            var depositResponse = await _client.ExecuteAsync(depositRequest);

            depositResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var newBalance = await GetCurrentBalanceAsync();
            newBalance.Should().Be(initialBalance); 
        }

        [Fact]
        public async Task WithdrawFunds_ShouldHandleNegativeAmount()
        {
            var initialBalance = await GetCurrentBalanceAsync();

            var withdrawAmount = -500; 
            var withdrawRequest = new RestRequest("/onlinewallet/withdraw", Method.Post);
            withdrawRequest.AddJsonBody(new { Amount = withdrawAmount });

            var withdrawResponse = await _client.ExecuteAsync(withdrawRequest);

            withdrawResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest); 
            var newBalance = await GetCurrentBalanceAsync();
            newBalance.Should().Be(initialBalance);
        }

        [Fact]
        public async Task DepositFunds_ShouldHandleLargeAmounts()
        {
            var initialBalance = await GetCurrentBalanceAsync();

            var depositAmount = 100_000_000; 
            var depositRequest = new RestRequest("/onlinewallet/deposit", Method.Post);
            depositRequest.AddJsonBody(new { Amount = depositAmount });

            var depositResponse = await _client.ExecuteAsync(depositRequest);
            depositResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var newBalance = await GetCurrentBalanceAsync();
            newBalance.Should().Be(initialBalance + depositAmount);
        }

        [Fact]
        public async Task WithdrawFunds_ShouldHandleLargeAmounts()
        {
            var withdrawAmount = 100_000_000; 
            var withdrawRequest = new RestRequest("/onlinewallet/withdraw", Method.Post);
            withdrawRequest.AddJsonBody(new { Amount = withdrawAmount });

            var withdrawResponse = await _client.ExecuteAsync(withdrawRequest);

            withdrawResponse.StatusCode.Should().Be(HttpStatusCode.OK); 
        }

        [Fact]
        public async Task DepositFunds_ShouldHandleZeroAmount()
        {
            var initialBalance = await GetCurrentBalanceAsync();

            var depositAmount = 0; 
            var depositRequest = new RestRequest("/onlinewallet/deposit", Method.Post);
            depositRequest.AddJsonBody(new { Amount = depositAmount });

            var depositResponse = await _client.ExecuteAsync(depositRequest);
            depositResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var newBalance = await GetCurrentBalanceAsync();
            newBalance.Should().Be(initialBalance); 
        }

        [Fact]
        public async Task WithdrawFunds_ShouldHandleZeroAmount()
        {
            var initialBalance = await GetCurrentBalanceAsync();
            var withdrawAmount = 0; 
            var withdrawRequest = new RestRequest("/onlinewallet/withdraw", Method.Post);
            withdrawRequest.AddJsonBody(new { Amount = withdrawAmount });

            var withdrawResponse = await _client.ExecuteAsync(withdrawRequest);
            withdrawResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var newBalance = await GetCurrentBalanceAsync();
            newBalance.Should().Be(initialBalance); 
        }

        [Fact]
        public async Task DepositFunds_ShouldFailWithoutAmount()
        {
            var depositRequest = new RestRequest("/onlinewallet/deposit", Method.Post);

            var depositResponse = await _client.ExecuteAsync(depositRequest);

            depositResponse.StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType); 
        }

        [Fact]
        public async Task WithdrawFunds_ShouldFailWithoutAmount()
        {
            var withdrawRequest = new RestRequest("/onlinewallet/withdraw", Method.Post);
            var withdrawResponse = await _client.ExecuteAsync(withdrawRequest);

            withdrawResponse.StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType); 
        }

        [Fact]
        public async Task DepositFunds_ShouldFailWithInvalidAmount()
        {
            var depositRequest = new RestRequest("/onlinewallet/deposit", Method.Post);
            depositRequest.AddJsonBody(new { Amount = "invalid" }); 

            var depositResponse = await _client.ExecuteAsync(depositRequest);

            depositResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task WithdrawFunds_ShouldFailWithInvalidAmount()
        {
            var withdrawRequest = new RestRequest("/onlinewallet/withdraw", Method.Post);
            withdrawRequest.AddJsonBody(new { Amount = "invalid" }); 

            var withdrawResponse = await _client.ExecuteAsync(withdrawRequest);

            withdrawResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest); 
        }
    
        public class BalanceResponse    {
        public decimal amount { get; set; }   }
    }
}
