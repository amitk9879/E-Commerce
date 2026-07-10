import { Component, OnInit, OnDestroy, inject, signal, computed, ViewChild, ElementRef } from '@angular/core';
import { Product } from '../../../shared/models/commerce.models';
import { ProductService, PaginatedResult } from '../../../core/services/product';
import { CartService } from '../../../core/services/cart';
import { ToastrService } from 'ngx-toastr';
import { environment } from '../../../../environments/environment';

const PAGE_SIZE = 10;

@Component({
  selector: 'app-catalog',
  standalone: true,
  templateUrl: './catalog.html',
  styleUrl: './catalog.css'
})
export class CatalogComponent implements OnInit, OnDestroy {
  @ViewChild('scrollSentinel') scrollSentinel!: ElementRef<HTMLDivElement>;

  private productService = inject(ProductService);
  private cartService = inject(CartService);
  private toastr = inject(ToastrService);

  private allProducts = signal<Product[]>([]);
  private currentPage = signal<number>(0);
  private totalCount = signal<number>(0);
  private intersectionObserver: IntersectionObserver | null = null;

  public searchText = signal<string>('');
  public loading = signal<boolean>(true);
  public loadingMore = signal<boolean>(false);
  public loadError = signal<boolean>(false);

  /** True when all pages have been fetched */
  public hasMore = computed(() => this.allProducts().length < this.totalCount());

  /** Public accessor for all loaded products */
  public products = computed(() => this.allProducts());

  /** Reactive map of productId → quantity in cart (0 = not in cart) */
  public cartQuantities = computed<Record<string, number>>(() => {
    const map: Record<string, number> = {};
    for (const item of this.cartService.cartList()) {
      map[item.product.id] = item.quantity;
    }
    return map;
  });

  public filteredProducts = computed(() => {
    const query = this.searchText().toLowerCase().trim();
    if (!query) return this.allProducts();

    return this.allProducts().filter(p =>
      p.name.toLowerCase().includes(query) ||
      p.description.toLowerCase().includes(query)
    );
  });

  ngOnInit(): void {
    this.loadNextPage();
    this.setupIntersectionObserver();
  }

  ngOnDestroy(): void {
    if (this.intersectionObserver) {
      this.intersectionObserver.disconnect();
    }
  }

  // ─── Infinite Scroll ───────────────────────────────────────────

  private setupIntersectionObserver(): void {
    const options = {
      root: null,
      rootMargin: '300px',
      threshold: 0.01
    };

    this.intersectionObserver = new IntersectionObserver((entries) => {
      entries.forEach(entry => {
        if (entry.isIntersecting && this.hasMore() && !this.loadingMore() && this.searchText().trim().length === 0) {
          this.loadNextPage();
        }
      });
    }, options);

    setTimeout(() => {
      if (this.scrollSentinel) {
        this.intersectionObserver?.observe(this.scrollSentinel.nativeElement);
      }
    }, 0);
  }

  public loadNextPage(): void {
    const nextPage = this.currentPage() + 1;
    const isFirstPage = nextPage === 1;

    if (isFirstPage) {
      this.loading.set(true);
    } else {
      this.loadingMore.set(true);
    }

    this.productService.getProductsPaginated(nextPage, PAGE_SIZE).subscribe({
      next: (result: PaginatedResult<Product>) => {
        this.currentPage.set(nextPage);
        this.totalCount.set(result.totalCount);
        this.allProducts.update(prev => [...prev, ...result.items]);
        this.loading.set(false);
        this.loadingMore.set(false);
        this.loadError.set(false);
      },
      error: (err) => {
        console.error('Failed to load products:', err);
        if (isFirstPage) {
          this.loadError.set(true);
          this.loading.set(false);
          this.toastr.error('Could not load products from server.', 'Network Error');
        } else {
          this.loadingMore.set(false);
          this.toastr.error('Could not load more products.', 'Network Error');
        }
      }
    });
  }

  // ─── Search ────────────────────────────────────────────────────

  public onSearchInput(event: Event): void {
    const inputElement = event.target as HTMLInputElement;
    this.searchText.set(inputElement.value);
  }

  /** Returns the quantity for a product (0 if not in cart) */
  public getCartQty(productId: string): number {
    return this.cartQuantities()[productId] ?? 0;
  }

  /**
   * Returns a product image URL, resolving relative paths against the API base.
   */
  public getImageUrl(product: Product): string {
    if (!product.imageUrl) return '';
    if (product.imageUrl.startsWith('http://') || product.imageUrl.startsWith('https://')) {
      return product.imageUrl;
    }
    const base = environment.apiGatewayUrl.replace(/\/+$/, '');
    const path = product.imageUrl.startsWith('/') ? product.imageUrl : `/${product.imageUrl}`;
    return `${base}${path}`;
  }

  /**
   * Safely derives a human-readable SKU from the product ID.
   */
  public getSku(product: Product): string {
    const parts = product.id.split('-');
    if (parts.length >= 2) {
      return parts[parts.length - 1] || 'GEN';
    }
    return product.id.substring(0, 6).toUpperCase();
  }

  /**
   * Handles image load errors by hiding the img element via a fallback flag.
   */
  public onImageError(event: Event): void {
    const img = event.target as HTMLImageElement;
    img.style.display = 'none';
    const container = img.parentElement;
    if (container) {
      container.classList.add('image-failed');
    }
  }

  /**
   * Triggers image retry after an error by showing a re-attempt indicator.
   */
  public onImageLoaded(event: Event): void {
    const img = event.target as HTMLImageElement;
    img.style.display = '';
    const container = img.parentElement;
    if (container) {
      container.classList.remove('image-failed');
    }
  }

  public onAddToCart(product: Product): void {
    if (product.stockQuantity <= 0) return;
    this.cartService.addToCart(product);
    this.toastr.info(`Added ${product.name} to your basket.`, 'Cart Updated');
  }

  /** Decrement quantity; removes item if quantity reaches 0 */
  public onDecrement(productId: string): void {
    this.cartService.updateQuantity(productId, -1);
  }

  /** Increment quantity (respects stock) */
  public onIncrement(product: Product): void {
    const currentQty = this.getCartQty(product.id);
    if (currentQty >= product.stockQuantity) {
      this.toastr.warning('No more stock available.', 'Limit Reached');
      return;
    }
    this.cartService.updateQuantity(product.id, 1);
  }
}
