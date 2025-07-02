using Microsoft.EntityFrameworkCore;
using sanda.Data;
using sanda.Models;
using sanda.Services;

public class WalletService : IWalletService
{
    private readonly UserDbContext _dbContext;

    public WalletService(UserDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Wallet> GetUserWalletAsync(int userId)
    {
        return await _dbContext.Wallets
            .FirstOrDefaultAsync(w => w.UserId == userId);
    }

    public async Task<Wallet> CreateWalletAsync(int userId)
    {
        var existingWallet = await GetUserWalletAsync(userId);
        if (existingWallet != null)
        {
            throw new Exception("User already has a wallet");
        }

        var wallet = new Wallet
        {
            UserId = userId,
            Balance = 0
        };

        _dbContext.Wallets.Add(wallet);
        await _dbContext.SaveChangesAsync();
        return wallet;
    }

    public async Task<Wallet> UpdateWalletBalanceAsync(int userId, decimal amount)
    {
        var wallet = await GetUserWalletAsync(userId) ??
            throw new Exception("Wallet not found");

        wallet.Balance = amount;
        wallet.UpdatedAt = DateTime.UtcNow;

        _dbContext.Wallets.Update(wallet);
        await _dbContext.SaveChangesAsync();
        return wallet;
    }

    public async Task<bool> DeleteWalletAsync(int userId)
    {
        var wallet = await GetUserWalletAsync(userId);
        if (wallet == null) return false;

        _dbContext.Wallets.Remove(wallet);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<decimal> GetBalanceAsync(int userId)
    {
        var wallet = await GetUserWalletAsync(userId) ??
            throw new Exception("Wallet not found");
        return wallet.Balance;
    }

    public async Task<Wallet> DepositAsync(int userId, decimal amount)
    {
        if (amount <= 0)
            throw new Exception("Deposit amount must be positive");

        var wallet = await GetUserWalletAsync(userId) ??
            throw new Exception("Wallet not found");

        wallet.Balance += amount;
        wallet.UpdatedAt = DateTime.UtcNow;

        _dbContext.Wallets.Update(wallet);
        await _dbContext.SaveChangesAsync();
        return wallet;
    }

    public async Task<Wallet> WithdrawAsync(int userId, decimal amount)
    {
        if (amount <= 0)
            throw new Exception("Withdrawal amount must be positive");

        var wallet = await GetUserWalletAsync(userId) ??
            throw new Exception("Wallet not found");

        if (wallet.Balance < amount)
            throw new Exception("Insufficient funds");

        wallet.Balance -= amount;
        wallet.UpdatedAt = DateTime.UtcNow;

        _dbContext.Wallets.Update(wallet);
        await _dbContext.SaveChangesAsync();
        return wallet;
    }
}
