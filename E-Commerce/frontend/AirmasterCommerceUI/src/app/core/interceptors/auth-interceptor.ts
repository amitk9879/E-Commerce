import { HttpInterceptorFn, HttpHeaders } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  
  // 1. Extract the secure authentication token from our active user session
  const token = authService.getAccessToken();

  // 2. Generate an active unique tracking string to feed our cross-domain Serilog trace logger matrix
  const correlationId = `CORR-UI-${crypto.randomUUID().substring(0, 8).toUpperCase()}`;

  // 3. Clone the request immutably to add tracking parameters and authorization headers safely
  let modifiedHeaders = req.headers
    .set('X-Correlation-ID', correlationId)
    .set('Accept', 'application/json');

  if (token) {
    modifiedHeaders = modifiedHeaders.set('Authorization', `Bearer ${token}`);
  }

  const authenticatedRequest = req.clone({ headers: modifiedHeaders });

  // 4. Pass the enhanced request down the execution pipeline stream to the Gateway proxy
  return next(authenticatedRequest);
};