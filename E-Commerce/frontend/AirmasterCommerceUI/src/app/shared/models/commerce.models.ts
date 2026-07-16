export interface SystemLog {
  timestamp: Date;
  level: 'INFORMATION' | 'WARNING' | 'EXCEPTION';
  service: string;
  correlationId: string;
  message: string;
}

export interface PlatformMetrics {
  ordersPerDay: number;
  conversionRate: number;
  systemLatencyMs: number;
  recommendations: string[];
}

export interface OrderItem {
  productId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
}

export interface Order {
  id: string;
  customerEmail: string;
  orderDate: Date;
  totalAmount: number;
  status: 'Submitted' | 'Processing' | 'Payment Approved' | 'Shipped' | 'Completed' | 'Failed';
  itemsCount: number;
  shippingAddress: string;
  transactionId?: string;
  trackingNumber?: string;
  carrier?: string;
  paidAtUtc?: Date;
  shippedAtUtc?: Date;
  items: OrderItem[];
}

export interface Product {
  id: string;
  name: string;
  description: string;
  price: number;
  imageUrl: string;
  category: string;
  stockQuantity: number;
}

export interface CartItem {
  product: Product;
  quantity: number;
}

export interface UserSession {
  userId: string;
  token: string;
  email: string;
  role: 'Customer' | 'Admin';
}