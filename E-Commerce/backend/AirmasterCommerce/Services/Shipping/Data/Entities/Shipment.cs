namespace Shipping.Data.Entities
{
    public class Shipment
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public string TrackingNumber { get; set; } = string.Empty;
        public string Carrier { get; set; } = "FedEx"; // Default mock carrier
        public string ShippingStatus { get; set; } = "LabelCreated"; // LabelCreated, InTransit, Delivered
        public DateTime CreatedAtUtc { get; set; }
    }
}
