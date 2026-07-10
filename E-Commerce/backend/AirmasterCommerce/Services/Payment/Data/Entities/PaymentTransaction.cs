namespace Payment.Data.Entities
{
    public class PaymentTransaction
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public string TransactionStatus { get; set; } = "Pending"; // Pending, Success, Failed
        public string GatewayReference { get; set; } = string.Empty; // Mock Stripe ID
        public DateTime ProcessedAtUtc { get; set; }
    }
}
