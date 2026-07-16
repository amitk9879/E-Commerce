using System;

namespace Catalog.API.Domain.Entities
{
    public class OrderReservation
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public decimal TotalAmount { get; set; }
        
        // Serialized JSON of OrderItemEventDto list to know what to restore on failure
        public string ReservedItemsJson { get; set; } = "[]";
        
        public DateTime ReservedAtUtc { get; set; }
    }
}
