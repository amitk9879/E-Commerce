import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { forkJoin } from 'rxjs';
import { finalize } from 'rxjs/operators';
import { AuthService } from '../../../core/services/auth';
import { OrderService } from '../../../core/services/order';
import { ProductService } from '../../../core/services/product';

export interface AdminDashboardMetrics {
  totalProducts: number;
  totalOrders: number;
  totalCustomers: number;
  totalRevenue: number;
  pendingOrders: number;
}

@Component({
  selector: 'app-analytics-cards',
  templateUrl: './analytics-cards.html',
  styleUrl: './analytics-cards.css',
  imports: [CommonModule]
})
export class AnalyticsCardsComponent implements OnInit {
  public metrics = signal<AdminDashboardMetrics | null>(null);
  public isLoading = false;

  private authService = inject(AuthService);
  private orderService = inject(OrderService);
  private productService = inject(ProductService);

  ngOnInit(): void {
    this.isLoading = true;

    forkJoin({
      orderMetrics: this.orderService.getOrderMetrics(),
      products: this.productService.getProductsPaginated(1, 1),
      userMetrics: this.authService.getUserMetrics()
    }).pipe(
      finalize(() => this.isLoading = false)
    ).subscribe({
      next: (data) => {
        const oMetrics = data.orderMetrics;
        const pMetrics = data.products; // Assuming PaginatedResult has TotalCount
        const uMetrics = data.userMetrics;

        this.metrics.set({
          totalProducts: pMetrics?.totalCount || 0,
          totalOrders: oMetrics?.totalOrders || 0,
          totalCustomers: uMetrics?.totalUsers || 0,
          totalRevenue: oMetrics?.totalRevenue || 0,
          pendingOrders: oMetrics?.pendingOrders || 0
        });
      },
      error: (err) => console.error('Error fetching dashboard analytics:', err)
    });
  }
}
