import { Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../core/services/auth';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule], // Perfectly streamlined: no legacy module pollution
  templateUrl: './login.html',
  styleUrls: ['./login.css']
})
export class LoginComponent {
  private fb = inject(FormBuilder).nonNullable; 
  private authService = inject(AuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private toastr = inject(ToastrService);

  public isSubmitting = signal<boolean>(false);

  // Strongly typed, non-nullable reactive form fields
  public loginForm = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(4)]]
  });

  public onLoginSubmit(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      this.toastr.warning('Please resolve form validation issues.', 'Validation Error');
      return;
    }

    this.isSubmitting.set(true);

    // Dynamic submission with type clarity out of the box
    this.authService.login(this.loginForm.getRawValue()).subscribe({
      next: () => {
        this.toastr.success('Logged in successfully!', 'Welcome Back');
        
        const returnUrl = this.route.snapshot.queryParams['returnUrl'];
        const targetRoute = returnUrl || (this.authService.currentUserRole() === 'Admin' ? '/admin/dashboard' : '/catalog');
        this.router.navigateByUrl(targetRoute);
      },
      error: (err) => {
        this.isSubmitting.set(false);
        console.error('Login error context:', err);
        
        if (err.status === 401) {
          this.toastr.error('Invalid email or password credentials.', 'Authentication Failed');
        } else {
          this.toastr.error('Unable to establish a link to the gateway server.', 'Network Error');
        }
      }
    });
  }
}