import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Product } from '../../../shared/models/commerce.models';
import { ProductService } from '../../../core/services/product';

@Component({
  selector: 'app-low-stock-products',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './low-stock-products.html'
})
export class LowStockProductsComponent implements OnInit {
  public lowStockProducts = signal<Product[]>([]);
  public isLoading = false;
  private productService = inject(ProductService);

  ngOnInit(): void {
    this.isLoading = true;
    this.productService.getLowStockProducts(5).subscribe({
      next: (products) => {
        this.lowStockProducts.set(products);
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error fetching low stock products:', err);
        this.isLoading = false;
      }
    });
  }
}
