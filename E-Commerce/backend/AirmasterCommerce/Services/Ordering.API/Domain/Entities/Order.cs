using Ordering.API.Domain.Enums;
using System;
using System.Collections.Generic;

namespace Ordering.API.Domain.Entities
{
    public class Order
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public string ShippingAddress { get; set; } = string.Empty;
        
        public DateTime? PaidAtUtc { get; set; }
        public DateTime? ShippedAtUtc { get; set; }
        
        public string? TransactionId { get; set; }
        public string? TrackingNumber { get; set; }
        public string? Carrier { get; set; }

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
