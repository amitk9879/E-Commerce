import { effect, Injectable, signal } from '@angular/core';
import { Product } from '../../shared/models/commerce.models';

@Injectable({
  providedIn: 'root'
})
export class CartService {

  private readonly STORAGE_KEY = 'airmaster_basket_state';

  // Strongly typed reactive signal matrix
  public cartList = signal<{ product: Product; quantity: number }[]>([]);

  constructor() {
    // 2. Setup an Angular Effect to automatically sync to localStorage whenever the signal changes
    effect(() => {
      localStorage.setItem(this.STORAGE_KEY, JSON.stringify(this.cartList()));
    });
  }

  public getCartCount(): number {
    return this.cartList().reduce((acc, item) => acc + item.quantity, 0);
  }

  public getCartTotal(): number {
    return this.cartList().reduce((acc, item) => acc + (item.product.price * item.quantity), 0);
  }

  /**
   * Adds a product instance reference or falls back to an incremental delta adjustment
   */
  public addToCart(product: Product): void {
    const current = this.cartList();
    const existingIndex = current.findIndex(item => item.product.id === product.id);

    if (existingIndex > -1) {
      this.updateQuantity(product.id, 1);
    } else {
      this.cartList.set([...current, { product, quantity: 1 }]);
    }
  }

  /**
   * Modifies quantities safely via relative additions or subtractions (-1 / +1)
   */
  public updateQuantity(productId: string, delta: number): void {
    const updated = this.cartList()
      .map(item => {
        if (item.product.id === productId) {
          return { ...item, quantity: item.quantity + delta };
        }
        return item;
      })
      .filter(item => item.quantity > 0); // Automatically discards item from list if quantity falls to 0

    this.cartList.set(updated);
  }

  /**
   * Synchronous quick-lookup query returning the item stack quantity
   */
  public getProductQuantity(productId: string): number {
    const match = this.cartList().find(item => item.product.id === productId);
    return match ? match.quantity : 0;
  }

  public clearCart(): void {
    this.cartList.set([]);
    localStorage.removeItem(this.STORAGE_KEY);
  }

  private loadCartFromStorage(): { product: Product; quantity: number }[] {
    try {
      const storedData = localStorage.getItem(this.STORAGE_KEY);
      return storedData ? JSON.parse(storedData) : [];
    } catch (e) {
      console.error('Failed to parse basket contents from local storage cache:', e);
      return [];
    }
  }
}