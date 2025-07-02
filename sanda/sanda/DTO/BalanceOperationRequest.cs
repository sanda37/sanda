using System.ComponentModel.DataAnnotations;

namespace sanda.DTO
{
    public class BalanceOperationRequest
    {
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be at least 0.01")]
        public decimal Amount { get; set; }
    }

}