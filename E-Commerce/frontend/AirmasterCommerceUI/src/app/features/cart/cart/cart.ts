import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { ReactiveFormsModule, FormControl, Validators } from '@angular/forms';
import { CartService } from '../../../core/services/cart';
import { AuthService } from '../../../core/services/auth';
import { ToastrService } from 'ngx-toastr';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule],
  templateUrl: './cart.html',
  styleUrl: './cart.css'
})
export class CartComponent {
  public cartService = inject(CartService);
  private authService = inject(AuthService);
  private http = inject(HttpClient);
  private router = inject(Router);
  private toastr = inject(ToastrService);

  public isProcessing = signal<boolean>(false);
  public shippingAddressControl = new FormControl('', {
    nonNullable: true,
    validators: [Validators.required]
  });

  public onCheckout(): void {
    if (!this.authService.isAuthenticated()) {
      this.toastr.warning('Please log in first to proceed.', 'Authentication Required');
      this.router.navigate(['/auth/login'], { queryParams: { returnUrl: '/cart' } });
      return;
    }

    if (this.shippingAddressControl.invalid) {
      this.shippingAddressControl.markAsTouched();
      this.toastr.warning('Please enter a valid shipping address to proceed.', 'Shipping Address Required');
      return;
    }

    this.isProcessing.set(true);

    const orderPayload = {
      userId: this.authService.currentUserId(),
      shippingAddress: this.shippingAddressControl.value.trim(),
      items: this.cartService.cartList().map((item: any) => ({
        productId: item.product.id,
        productName: item.product.name,
        quantity: item.quantity,
        unitPrice: item.product.price
      }))
    };

    this.http.post(`${environment.orderingEndpoint}`, orderPayload).subscribe({
      next: (res: any) => {
        this.toastr.success(res.message || 'Order placed successfully!', 'Success');
        this.cartService.clearCart();
        this.isProcessing.set(false);
        this.router.navigate(['/customer/orders']);
      },
      error: (err) => {
        this.isProcessing.set(false);
        console.error('Checkout error stack:', err);
        this.toastr.error('Transaction failed to save. Verify Gateway routing rules.', 'Gateway Failure');
      }
    });
  }
}