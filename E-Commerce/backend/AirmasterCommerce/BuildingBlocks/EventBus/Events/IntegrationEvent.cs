using System;
using System.Collections.Generic;
using System.Text;

namespace EventBus.Events
{
    public record IntegrationEvent
    {
        public Guid Id { get; init; }
        public DateTime CreationDate { get; init; }
        public string? CorrelationId { get; init; }

        public IntegrationEvent()
        {
            Id = Guid.NewGuid();
            CreationDate = DateTime.UtcNow;
            CorrelationId = Guid.NewGuid().ToString();
        }

        // Required for proper JSON deserialization in background workers
        public IntegrationEvent(Guid id, DateTime creationDate, string? correlationId = null)
        {
            Id = id;
            CreationDate = creationDate;
            CorrelationId = correlationId;
        }
    }
}
