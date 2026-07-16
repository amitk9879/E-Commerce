using EventBus.Events;
using System;

namespace Ordering.API.Infrastructure.BackgroundWorkers
{
    // Minimal integration event contracts required by the Ordering service
    public record PaymentCompletedIntegrationEvent(Guid OrderId, Guid UserId, string TransactionId) : IntegrationEvent;
    public record PaymentFailedIntegrationEvent(Guid OrderId, string Reason) : IntegrationEvent;

    public record ShipmentCreatedIntegrationEvent(Guid OrderId, string TrackingNumber, string Carrier, string TransactionId) : IntegrationEvent;
    public record ShippingFailedIntegrationEvent(Guid OrderId, string TransactionId, string Reason) : IntegrationEvent;
}
