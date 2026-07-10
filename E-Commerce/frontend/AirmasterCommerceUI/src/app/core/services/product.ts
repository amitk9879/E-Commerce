import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { shareReplay } from 'rxjs/operators';
import { Product } from '../../shared/models/commerce.models';
import { environment } from '../../../environments/environment';

export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  private http = inject(HttpClient);
  private _lowStockCache$?: Observable<Product[]>;
  
  private baseUrl = `${environment.catalogEndpoint}/products`; 

  public getAllProducts(): Observable<Product[]> {
    return this.http.get<Product[]>(this.baseUrl);
  }

  public getProductsPaginated(page: number, pageSize: number): Observable<PaginatedResult<Product>> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    return this.http.get<PaginatedResult<Product>>(this.baseUrl, { params });
  }

  public getLowStockProducts(limit: number = 5): Observable<Product[]> {
    if (!this._lowStockCache$) {
      this._lowStockCache$ = this.http.get<Product[]>(`${this.baseUrl}/low-stock`, {
        params: new HttpParams().set('limit', limit.toString())
      }).pipe(
        shareReplay(1)
      );
    }
    return this._lowStockCache$;
  }
}
