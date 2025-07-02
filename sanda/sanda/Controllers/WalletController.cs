using Microsoft.AspNetCore.Mvc;
using sanda.Models;
using sanda.Services;
using System.ComponentModel.DataAnnotations;

namespace sanda.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _walletService;

        public WalletController(IWalletService walletService)
        {
            _walletService = walletService;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetWallet(int userId)
        {
            try
            {
                var wallet = await _walletService.GetUserWalletAsync(userId);
                if (wallet == null)
                    return NotFound(new { message = "Wallet not found" });

                return Ok(wallet);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{userId}")]
        public async Task<IActionResult> CreateWallet(int userId)
        {
            try
            {
                var wallet = await _walletService.CreateWalletAsync(userId);
                return CreatedAtAction(nameof(GetWallet), new { userId = userId }, wallet);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        //[HttpPut("{userId}/balance")]
        //    public async Task<IActionResult> UpdateBalance(
        //        int userId,
        //        [FromBody][Range(0, double.MaxValue)] decimal amount)
        //    {
        //        try
        //        {
        //            var wallet = await _walletService.UpdateWalletBalanceAsync(userId, amount);
        //            return Ok(wallet);
        //        }
        //        catch (Exception ex)
        //        {
        //            return BadRequest(new { message = ex.Message });
        //        }
        //    }

        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteWallet(int userId)
        {
            try
            {
                var result = await _walletService.DeleteWalletAsync(userId);
                if (!result)
                    return NotFound(new { message = "Wallet not found" });

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet("{userId}/balance")]
        public async Task<IActionResult> GetBalance(int userId)
        {
            try
            {
                var balance = await _walletService.GetBalanceAsync(userId);
                return Ok(new { userId, balance });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpPost("{userId}/deposit")]
        public async Task<IActionResult> Deposit(
          int userId,
          [FromBody] DepositRequest request) // Changed to use a request object
        {
            try
            {
                var wallet = await _walletService.DepositAsync(userId, request.Amount);
                return Ok(wallet);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        public class DepositRequest
        {
            [Range(0.01, double.MaxValue)]
            public decimal Amount { get; set; }
        }

        [HttpPost("{userId}/withdraw")]
        public async Task<IActionResult> Withdraw(
        int userId,
        [FromBody] WithdrawRequest request)
        {
            try
            {
                var wallet = await _walletService.WithdrawAsync(userId, request.Amount);
                return Ok(wallet);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        public class WithdrawRequest
        {
            [Range(0.01, double.MaxValue)]
            public decimal Amount { get; set; }
        }
    }
}