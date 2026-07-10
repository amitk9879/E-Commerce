import { Component, inject, computed, signal } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth';
import { CartService } from '../../../core/services/cart';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterModule],
  templateUrl: './navbar.html',
  styleUrls: ['./navbar.css']
})
export class NavbarComponent {
  public authService = inject(AuthService);
  public cartService = inject(CartService); 
  private router = inject(Router);

  // 1. Manages whether the sidebar drawer slides into view
  public isCartOpen = signal<boolean>(false);

  // 2. Reactively maps your totals and cart items arrays from CartService
  public cartCount = computed(() => this.cartService.getCartCount());
  public cartItems = computed(() => this.cartService.cartList());
  
  public cartTotal = computed(() => {
    return this.cartItems().reduce((acc, item) => acc + (item.product.price * item.quantity), 0);
  });

  // 3. Toggles the backdrop filter / drawer wrapper view
  public toggleCart(isOpen: boolean): void {
    this.isCartOpen.set(isOpen);
  }

  // 4. Modifies the quantity of items directly inside the side drawer rows
  public onUpdateQty(productId: string, delta: number): void {
    this.cartService.updateQuantity(productId, delta);
  }

  public onProceedToCheckout(): void {
    this.toggleCart(false);
    this.router.navigate(['/cart']);
  }

  public onLogout(): void {
    this.authService.logout(); 
    this.router.navigate(['/auth/login']);
  }
}