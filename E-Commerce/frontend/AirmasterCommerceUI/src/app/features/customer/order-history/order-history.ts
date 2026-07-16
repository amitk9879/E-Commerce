import { Component, OnInit, OnDestroy, inject, signal, effect, untracked } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Order } from '../../../shared/models/commerce.models';
import { AuthService } from '../../../core/services/auth';
import { NotificationService } from '../../../core/services/notification';
import { OrderService } from '../../../core/services/order';

@Component({
  selector: 'app-order-history',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './order-history.html',
  styleUrl: './order-history.css'
})
export class OrderHistoryComponent implements OnInit, OnDestroy {
  private authService = inject(AuthService);
  private notificationService = inject(NotificationService);
  private orderService = inject(OrderService);

  // Core array containing tracking records
  public ordersList = signal<Order[]>([]);

  // Tracks the currently selected order for the details modal popup
  public selectedOrder = signal<Order | null>(null);

  constructor() {
    // Reactive Watcher: Runs automatically whenever latestOrderStatusUpdate changes value
    effect(() => {
      const liveUpdate = this.notificationService.latestOrderStatusUpdate();
      if (liveUpdate) {
        // untracked() prevents this effect from tracking ordersList/selectedOrder as dependencies,
        // which would cause an infinite re-trigger loop (read → write → re-run → read → write...)
        untracked(() => {
          this.updateLocalOrderStatus(liveUpdate.orderId, liveUpdate.status as any, liveUpdate);
        });
      }
    }, { allowSignalWrites: true });
  }

  ngOnInit(): void {
    console.log('🔥 ngOnInit Triggered! Component created.'); // Test 1
    this.loadUserOrders();
    // Connect the live background websocket stream
    this.notificationService.startConnection();
  }

  ngOnDestroy(): void {
    // Terminate connection loops cleanly upon navigation exit to protect browser memory
    this.notificationService.stopConnection();
  }

  public onOrderClick(order: Order): void {
    this.selectedOrder.set(order);
  }

  public closeModal(): void {
    this.selectedOrder.set(null);
  }

  private loadUserOrders(): void {
    const userId = this.authService.currentUserId();
    if (!userId) return;
    console.log('🚀 Firing HTTP Request...'); // Test 2

    this.orderService.getUserOrders(userId).subscribe({
      next: (orders) => {
        this.ordersList.set(orders);
        
        // If the modal is currently open, refresh the details inside it
        const activeOrder = this.selectedOrder();
        if (activeOrder) {
          const updated = orders.find(o => o.id === activeOrder.id);
          if (updated) {
            this.selectedOrder.set(updated);
          }
        }
      },
      error: (err) => {
        console.error('Failed to load user orders from DB:', err);
      }
    });
  }

  /**
   * Updates state data completely in-memory using incoming websocket payload definitions
   * instead of scheduling recurring HTTP query loops.
   */
  private updateLocalOrderStatus(orderId: string, newStatus: Order['status'], livePayload: any): void {
    const currentOrders = this.ordersList();
    const orderIndex = currentOrders.findIndex(o => o.id === orderId);
    
    if (orderIndex > -1) {
      // Create a brand new deep array reference copy to safely trigger Signal mutations
      const updatedOrders = [...currentOrders];
      // Prevent downgrading status if websocket messages arrive out of order (e.g. Payment arriving after Shipped)
      let resolvedStatus = newStatus;
      const currentStatus = updatedOrders[orderIndex].status;
      if (currentStatus === 'Shipped' && newStatus === 'Payment Approved') {
        resolvedStatus = 'Shipped';
      }

      // Merge status along with newly provided websocket logistics details (trackingNumber, transactionId, etc.)
      updatedOrders[orderIndex] = {
        ...updatedOrders[orderIndex],
        status: resolvedStatus,
        trackingNumber: livePayload.trackingNumber || updatedOrders[orderIndex].trackingNumber,
        transactionId: livePayload.transactionId || updatedOrders[orderIndex].transactionId,
        carrier: livePayload.carrier || updatedOrders[orderIndex].carrier,
        paidAtUtc: resolvedStatus === 'Payment Approved' ? new Date() : updatedOrders[orderIndex].paidAtUtc,
        shippedAtUtc: resolvedStatus === 'Shipped' ? new Date() : updatedOrders[orderIndex].shippedAtUtc
      };

      // Set the updated list reference array
      this.ordersList.set(updatedOrders);

      // If this order is open in the modal, mirror the change immediately
      const activeOrder = this.selectedOrder();
      if (activeOrder && activeOrder.id === orderId) {
        this.selectedOrder.set(updatedOrders[orderIndex]);
      }
      
    }
  }
}