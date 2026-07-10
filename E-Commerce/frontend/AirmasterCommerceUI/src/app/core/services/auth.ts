import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from "../../../environments/environment";

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);

  // Reactive state signals accessible across components
  public currentUserEmail = signal<string>('Customer');
  public currentUserRole = signal<string>('Customer');
  public currentUserId = signal<string>('');

  private readonly ACCESS_TOKEN_KEY = 'airmaster_access_token';
  private readonly REFRESH_TOKEN_KEY = 'airmaster_refresh_token';

  /**
   * Dispatches login parameters directly to the YARP Gateway endpoint
   */
  constructor() {
    const token = this.getAccessToken();
    if (token) {
      this.decodeAndPublishUserClaims(token);
    }
  }

  public login(credentials: { email: string; password: string }): Observable<any> {
    return this.http.post<any>(`${environment.authEndpoint}/login`, credentials).pipe(
      tap(response => {
        if (response && response.accessToken) {
          this.saveTokens(response.accessToken, response.refreshToken);
          this.decodeAndPublishUserClaims(response.accessToken);
        }
      })
    );
  }

  public getAllUsers(page: number = 1, pageSize: number = 10): Observable<any> {
    return this.http.get<any>(`${environment.authEndpoint}/users?page=${page}&pageSize=${pageSize}`);
  }

  public getUserMetrics(): Observable<any> {
    return this.http.get<any>(`${environment.authEndpoint}/users/metrics`);
  }

  public isAuthenticated(): boolean {
    return !!this.getAccessToken();
  }

  public getAccessToken(): string | null {
    return localStorage.getItem(this.ACCESS_TOKEN_KEY);
  }

  public logout(): void {
    localStorage.removeItem(this.ACCESS_TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_TOKEN_KEY);
    this.currentUserEmail.set('Customer');
    this.currentUserRole.set('Customer');
    this.currentUserId.set('');
  }

  private saveTokens(accessToken: string, refreshToken: string): void {
    localStorage.setItem(this.ACCESS_TOKEN_KEY, accessToken);
    localStorage.setItem(this.REFRESH_TOKEN_KEY, refreshToken);
  }

  /**
   * Decodes JWT payload claims to resolve user identification details
   */
  private decodeAndPublishUserClaims(token: string): void {
    try {
      const payloadBase64 = token.split('.')[1];
      const decodedJson = JSON.parse(atob(payloadBase64));

      // Mapped to match standard ASP.NET Core JWT Claim types
      const userId = decodedJson['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] || decodedJson['sub'];
      const email = decodedJson['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] || decodedJson['email'];
      const role = decodedJson['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || decodedJson['role'];

      if (userId) this.currentUserId.set(userId);
      if (email) this.currentUserEmail.set(email);
      if (role) this.currentUserRole.set(role);
    } catch (error) {
      console.error('Failed decoding identity context token:', error);
    }
  }
}