import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../core/services/auth';

@Component({
  selector: 'app-admin-customers',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './customers.html'
})
export class AdminCustomersComponent implements OnInit {
  public customers = signal<any[]>([]);
  public loading = signal<boolean>(true);
  public currentPage = signal<number>(1);
  public totalPages = signal<number>(1);
  private pageSize = 10;
  
  private authService = inject(AuthService);

  ngOnInit(): void {
    this.loadCustomers();
  }

  public loadCustomers(page: number = 1): void {
    this.loading.set(true);
    this.currentPage.set(page);
    this.authService.getAllUsers(page, this.pageSize).subscribe({
      next: (data) => {
        this.customers.set(data.items);
        this.totalPages.set(data.totalPages);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error fetching customers', err);
        this.loading.set(false);
      }
    });
  }

  public nextPage(): void {
    if (this.currentPage() < this.totalPages()) {
      this.loadCustomers(this.currentPage() + 1);
    }
  }

  public prevPage(): void {
    if (this.currentPage() > 1) {
      this.loadCustomers(this.currentPage() - 1);
    }
  }
}
