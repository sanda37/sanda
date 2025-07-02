using sanda.Data;
using Microsoft.EntityFrameworkCore;
using sanda.Models;

namespace sanda.Services
{
    public interface IVolunteerBalanceService
    {
        Task<Volunteer> GetVolunteerAsync(int volunteerId);
        Task<decimal> GetBalanceAsync(int volunteerId);
        Task<Volunteer> DepositAsync(int volunteerId, decimal amount);
        Task<Volunteer> WithdrawAsync(int volunteerId, decimal amount);
        Task<bool> CanWithdrawAsync(int volunteerId, decimal amount);
    }

    public class VolunteerBalanceService : IVolunteerBalanceService
    {
        private readonly UserDbContext _dbContext;

        public VolunteerBalanceService(UserDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Volunteer> GetVolunteerAsync(int volunteerId)
        {
            return await _dbContext.Volunteers
                .FirstOrDefaultAsync(v => v.ID == volunteerId)
                ?? throw new Exception("Volunteer not found");
        }

        public async Task<decimal> GetBalanceAsync(int volunteerId)
        {
            var volunteer = await GetVolunteerAsync(volunteerId);
            return volunteer.Balance;
        }

        public async Task<Volunteer> DepositAsync(int volunteerId, decimal amount)
        {
            if (amount <= 0)
                throw new Exception("Deposit amount must be positive");

            var volunteer = await GetVolunteerAsync(volunteerId);
            volunteer.Balance += amount;

            _dbContext.Volunteers.Update(volunteer);
            await _dbContext.SaveChangesAsync();
            return volunteer;
        }

        public async Task<Volunteer> WithdrawAsync(int volunteerId, decimal amount)
        {
            if (amount <= 0)
                throw new Exception("Withdrawal amount must be positive");

            var volunteer = await GetVolunteerAsync(volunteerId);

            if (volunteer.Balance < amount)
                throw new Exception("Insufficient funds");

            volunteer.Balance -= amount;
            _dbContext.Volunteers.Update(volunteer);
            await _dbContext.SaveChangesAsync();
            return volunteer;
        }

        public async Task<bool> CanWithdrawAsync(int volunteerId, decimal amount)
        {
            var volunteer = await GetVolunteerAsync(volunteerId);
            return volunteer.Balance >= amount;
        }
    }
}