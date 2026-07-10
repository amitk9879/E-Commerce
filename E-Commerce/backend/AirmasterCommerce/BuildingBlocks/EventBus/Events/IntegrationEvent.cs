using System;
using System.Collections.Generic;
using System.Text;

namespace EventBus.Events
{
    public record IntegrationEvent
    {
        public Guid Id { get; init; }
        public DateTime CreationDate { get; init; }

        public IntegrationEvent()
        {
            Id = Guid.NewGuid();
            CreationDate = DateTime.UtcNow;
        }

        // Required for proper JSON deserialization in background workers
        public IntegrationEvent(Guid id, DateTime creationDate)
        {
            Id = id;
            CreationDate = creationDate;
        }
    }
}
