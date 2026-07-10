import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Order } from '../../../shared/models/commerce.models';
import { OrderService } from '../../../core/services/order';

@Component({
  selector: 'app-admin-orders',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './orders.html'
})
export class AdminOrdersComponent implements OnInit {
  public orders = signal<Order[]>([]);
  public loading = signal<boolean>(true);
  public currentPage = signal<number>(1);
  public totalPages = signal<number>(1);
  private pageSize = 10;
  
  private orderService = inject(OrderService);

  ngOnInit(): void {
    this.loadOrders();
  }

  public loadOrders(page: number = 1): void {
    this.loading.set(true);
    this.currentPage.set(page);
    this.orderService.getAllOrders(page, this.pageSize).subscribe({
      next: (data) => {
        this.orders.set(data.items);
        this.totalPages.set(data.totalPages);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error fetching orders', err);
        this.loading.set(false);
      }
    });
  }

  public nextPage(): void {
    if (this.currentPage() < this.totalPages()) {
      this.loadOrders(this.currentPage() + 1);
    }
  }

  public prevPage(): void {
    if (this.currentPage() > 1) {
      this.loadOrders(this.currentPage() - 1);
    }
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
