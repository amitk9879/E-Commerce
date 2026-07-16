using System;

namespace Shipping.Data.Entities
{
    public class IdempotencyRecord
    {
        public Guid EventId { get; set; }
        public string EventType { get; set; } = string.Empty;
        public DateTime ProcessedAtUtc { get; set; }
    }
}
