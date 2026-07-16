using System;

namespace Catalog.API.Domain.Entities
{
    public class IdempotencyRecord
    {
        public Guid EventId { get; set; }
        public string EventType { get; set; } = string.Empty;
        public DateTime ProcessedAtUtc { get; set; }
    }
}
