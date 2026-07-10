import { Component } from '@angular/core';
import { AnalyticsCardsComponent } from '../analytics-cards/analytics-cards';
import { OrderManagementComponent } from '../order-management/order-management';
import { LowStockProductsComponent } from '../low-stock-products/low-stock-products';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [AnalyticsCardsComponent, OrderManagementComponent, LowStockProductsComponent],
  templateUrl: './dashboard.html'
})
export class DashboardComponent {}