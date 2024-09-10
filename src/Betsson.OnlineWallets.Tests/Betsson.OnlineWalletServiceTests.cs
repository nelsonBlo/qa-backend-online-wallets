using Moq;
using Betsson.OnlineWallets.Data.Models;
using Betsson.OnlineWallets.Data.Repositories;
using Betsson.OnlineWallets.Exceptions;
using Betsson.OnlineWallets.Models;
using Betsson.OnlineWallets.Services;
using FluentAssertions;
using Xunit;

namespace Betsson.OnlineWallets.Tests
{
    public class BetssonOnlineWalletServiceTests
    {
        private readonly Mock<IOnlineWalletRepository> _mockRepository;
        private readonly OnlineWalletService _service;

        public BetssonOnlineWalletServiceTests()
        {
            _mockRepository = new Mock<IOnlineWalletRepository>();
            _service = new OnlineWalletService(_mockRepository.Object);
        }

        [Fact]
        public async Task GetBalanceAsync_ShouldReturnCorrectBalance_WhenThereAreEntries()
        {
            var lastEntry = new OnlineWalletEntry { Amount = 100, BalanceBefore = 50 };
            _mockRepository.Setup(r => r.GetLastOnlineWalletEntryAsync()).ReturnsAsync(lastEntry);

            var result = await _service.GetBalanceAsync();

            result.Amount.Should().Be(150); // 50 + 100
        }

        [Fact]
        public async Task DepositFundsAsync_ShouldIncreaseBalance()
        {
            var currentBalance = new Balance { Amount = 100 };
            var deposit = new Deposit { Amount = 50 };
            _mockRepository.Setup(r => r.GetLastOnlineWalletEntryAsync()).ReturnsAsync(new OnlineWalletEntry { Amount = 0, BalanceBefore = 100 });
            _mockRepository.Setup(r => r.InsertOnlineWalletEntryAsync(It.IsAny<OnlineWalletEntry>())).Returns(Task.CompletedTask);

            var result = await _service.DepositFundsAsync(deposit);

            result.Amount.Should().Be(150); // 100 + 50
        }

        [Fact]
        public async Task WithdrawFundsAsync_ShouldDecreaseBalance_WhenSufficientFunds()
        {
            var currentBalance = new Balance { Amount = 100 };
            var withdrawal = new Withdrawal { Amount = 50 };
            _mockRepository.Setup(r => r.GetLastOnlineWalletEntryAsync()).ReturnsAsync(new OnlineWalletEntry { Amount = 0, BalanceBefore = 100 });
            _mockRepository.Setup(r => r.InsertOnlineWalletEntryAsync(It.IsAny<OnlineWalletEntry>())).Returns(Task.CompletedTask);

            var result = await _service.WithdrawFundsAsync(withdrawal);

            result.Amount.Should().Be(50); // 100 - 50
        }

        [Fact]
        public async Task WithdrawFundsAsync_ShouldThrowException_WhenInsufficientFunds()
        {
            var withdrawal = new Withdrawal { Amount = 200 };
            _mockRepository.Setup(r => r.GetLastOnlineWalletEntryAsync()).ReturnsAsync(new OnlineWalletEntry { Amount = 0, BalanceBefore = 100 });

            Func<Task> act = async () => await _service.WithdrawFundsAsync(withdrawal);

            await act.Should().ThrowAsync<InsufficientBalanceException>();
        }

        [Fact]
        public async Task GetBalanceAsync_ShouldReturnZero_WhenNoEntriesExist()
        {
            _mockRepository.Setup(r => r.GetLastOnlineWalletEntryAsync()).ReturnsAsync((OnlineWalletEntry)null);

            var result = await _service.GetBalanceAsync();

            result.Amount.Should().Be(0); // No entries, so balance should be 0
        }

        [Fact]
        public async Task DepositFundsAsync_ShouldHandleNegativeDeposit()
        {
            var negativeDeposit = new Deposit { Amount = -50 };
            var currentBalance = new Balance { Amount = 100 };
            _mockRepository.Setup(r => r.GetLastOnlineWalletEntryAsync()).ReturnsAsync(new OnlineWalletEntry { Amount = 100, BalanceBefore = 0 });
            _mockRepository.Setup(r => r.InsertOnlineWalletEntryAsync(It.IsAny<OnlineWalletEntry>())).Returns(Task.CompletedTask);

            var result = await _service.DepositFundsAsync(negativeDeposit);

            result.Amount.Should().Be(50); // 100 - 50
        }
   
        [Fact]
        public async Task GetBalanceAsync_ShouldReturnCorrectBalance_WhenMultipleEntriesExist()
        {
            var entries = new List<OnlineWalletEntry>
            {
                new OnlineWalletEntry { Amount = 100, BalanceBefore = 0 },
                new OnlineWalletEntry { Amount = -50, BalanceBefore = 100 }
            };
            _mockRepository.Setup(r => r.GetLastOnlineWalletEntryAsync()).ReturnsAsync(entries.Last());

            var result = await _service.GetBalanceAsync();

            result.Amount.Should().Be(50); // Final balance after all entries
        }

        [Fact]
        public async Task DepositFundsAsync_ShouldThrowException_WhenRepositoryFails()
        {
            var deposit = new Deposit { Amount = 50 };
            _mockRepository.Setup(r => r.InsertOnlineWalletEntryAsync(It.IsAny<OnlineWalletEntry>())).ThrowsAsync(new Exception("Repository error"));

            Func<Task> act = async () => await _service.DepositFundsAsync(deposit);

            await act.Should().ThrowAsync<Exception>().WithMessage("Repository error");
        }

        [Fact]
        public async Task DepositFundsAsync_ShouldSetEventTime_WhenDepositIsMade()
        {
            var deposit = new Deposit { Amount = 50 };
            var currentBalance = new Balance { Amount = 100 };
            var now = DateTimeOffset.UtcNow;
            _mockRepository.Setup(r => r.GetLastOnlineWalletEntryAsync()).ReturnsAsync(new OnlineWalletEntry { Amount = 0, BalanceBefore = 100 });
            _mockRepository.Setup(r => r.InsertOnlineWalletEntryAsync(It.IsAny<OnlineWalletEntry>())).Returns(Task.CompletedTask);

            var result = await _service.DepositFundsAsync(deposit);

            _mockRepository.Verify(r => r.InsertOnlineWalletEntryAsync(It.Is<OnlineWalletEntry>(e => e.EventTime.Date == now.Date)), Times.Once);
        }

    }
}
