import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Product } from '../../../shared/models/commerce.models';
import { ProductService } from '../../../core/services/product';

@Component({
  selector: 'app-admin-products',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './products.html'
})
export class AdminProductsComponent implements OnInit {
  public products = signal<Product[]>([]);
  public loading = signal<boolean>(true);
  public currentPage = signal<number>(1);
  public totalPages = signal<number>(1);
  private pageSize = 10;

  private productService = inject(ProductService);

  ngOnInit(): void {
    this.loadProducts();
  }

  public loadProducts(page: number = 1): void {
    this.loading.set(true);
    this.currentPage.set(page);
    this.productService.getProductsPaginated(page, this.pageSize).subscribe({
      next: (data) => {
        this.products.set(data.items);
        this.totalPages.set(data.totalPages);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error fetching products', err);
        this.loading.set(false);
      }
    });
  }

  public nextPage(): void {
    if (this.currentPage() < this.totalPages()) {
      this.loadProducts(this.currentPage() + 1);
    }
  }

  public prevPage(): void {
    if (this.currentPage() > 1) {
      this.loadProducts(this.currentPage() - 1);
    }
  }
}
