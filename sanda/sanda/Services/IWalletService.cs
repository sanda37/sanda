using sanda.Data;
using sanda.Models;
using Microsoft.EntityFrameworkCore;

namespace sanda.Services
{
    public interface IWalletService
    {
        Task<Wallet> GetUserWalletAsync(int userId);
        Task<Wallet> CreateWalletAsync(int userId);
        Task<Wallet> UpdateWalletBalanceAsync(int userId, decimal amount);
        Task<bool> DeleteWalletAsync(int userId);
        Task<decimal> GetBalanceAsync(int userId);
        Task<Wallet> DepositAsync(int userId, decimal amount);
        Task<Wallet> WithdrawAsync(int userId, decimal amount);
    }
}