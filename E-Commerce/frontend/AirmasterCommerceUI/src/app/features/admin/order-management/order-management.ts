import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Order } from '../../../shared/models/commerce.models';
import { OrderService } from '../../../core/services/order';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-order-management',
  imports: [CommonModule, RouterModule],
  templateUrl: './order-management.html'
})
export class OrderManagementComponent implements OnInit {
  public recentOrders = signal<Order[]>([]);
  public isLoading = false;
  public selectedOrder = signal<Order | null>(null);

  private orderService = inject(OrderService);
  private toastr = inject(ToastrService);

  ngOnInit(): void {
    this.isLoading = true;
    this.orderService.getRecentOrders(5).subscribe({
      next: (orders) => {
        this.recentOrders.set(orders);
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error fetching recent orders:', err);
        this.toastr.error('Could not load recent orders.');
        this.isLoading = false;
      }
    });
  }

  public openOrderDetails(order: Order, event: Event): void {
    event.preventDefault();
    this.selectedOrder.set(order);
  }

  public closeOrderDetails(): void {
    this.selectedOrder.set(null);
  }

  public getStatusClass(status: string): string {
    switch (status) {
      case 'Completed':
      case 'Shipped':
      case 'Payment Approved':
        return 'bg-success text-white';
      case 'Processing':
      case 'Submitted':
        return 'bg-warning text-dark';
      case 'Failed':
        return 'bg-danger text-white';
      default:
        return 'bg-secondary text-white';
    }
  }
}