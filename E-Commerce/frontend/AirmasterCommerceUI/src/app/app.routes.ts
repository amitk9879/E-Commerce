import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'auth/login',
    loadComponent: () => import('./features/auth/login/login').then(m => m.LoginComponent)
  },
  {
    path: 'catalog',
    loadComponent: () => import('./features/catalog/catalog/catalog').then(m => m.CatalogComponent)
  },
  {
    path: 'cart',
    loadComponent: () => import('./features/cart/cart/cart').then(m => m.CartComponent)
  },
  {
    path: 'customer/orders',
    loadComponent: () => import('./features/customer/order-history/order-history').then(m => m.OrderHistoryComponent)
  },
  
  // Admin Routes
  {
    path: 'admin/dashboard',
    loadComponent: () => import('./features/admin/dashboard/dashboard').then(m => m.DashboardComponent)
  },
  {
    path: 'admin/products',
    loadComponent: () => import('./features/admin/products/products').then(m => m.AdminProductsComponent)
  },
  {
    path: 'admin/orders',
    loadComponent: () => import('./features/admin/orders/orders').then(m => m.AdminOrdersComponent)
  },
  {
    path: 'admin/customers',
    loadComponent: () => import('./features/admin/customers/customers').then(m => m.AdminCustomersComponent)
  },
  
  {
    path: '',
    redirectTo: 'catalog',
    pathMatch: 'full'
  }
];