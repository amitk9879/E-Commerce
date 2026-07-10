import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { shareReplay } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class OrderService {
  private _metricsCache$?: Observable<any>;
  private _recentOrdersCache$?: Observable<any>;
  private http = inject(HttpClient);

  public submitOrder(orderData: any): Observable<any> {
    return this.http.post(`${environment.orderingEndpoint}/submit`, orderData);
  }

  public getUserOrders(userId: string): Observable<any[]> {
    return this.http.get<any[]>(`${environment.orderingEndpoint}/user/${userId}`);
  }

  public getAllOrders(page: number = 1, pageSize: number = 10): Observable<any> {
    return this.http.get<any>(`${environment.orderingEndpoint}/all?page=${page}&pageSize=${pageSize}`);
  }

  public getRecentOrders(limit: number = 5): Observable<any[]> {
    if (!this._recentOrdersCache$) {
      this._recentOrdersCache$ = this.http.get<any[]>(`${environment.orderingEndpoint}/recent?limit=${limit}`).pipe(
        shareReplay(1)
      );
    }
    return this._recentOrdersCache$;
  }

  public getOrderMetrics(): Observable<any> {
    if (!this._metricsCache$) {
      this._metricsCache$ = this.http.get<any>(`${environment.orderingEndpoint}/metrics`).pipe(
        shareReplay(1)
      );
    }
    return this._metricsCache$;
  }
}
