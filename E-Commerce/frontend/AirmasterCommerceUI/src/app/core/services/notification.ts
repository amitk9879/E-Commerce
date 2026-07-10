import { Injectable, inject, signal } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { ToastrService } from 'ngx-toastr';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private toastr = inject(ToastrService);
  private hubConnection!: signalR.HubConnection;

  // Signal exposed to components to notify them when an order updates live
  public latestOrderStatusUpdate = signal<{ 
    orderId: string; 
    status: string; 
    trackingNumber?: string;
    transactionId?: string;
    carrier?: string;
  } | null>(null);

  public startConnection(): void {
    // 🟢 CRITICAL SAFEGUARD: If connection is already built and alive/connecting, DO NOT duplicate it
    if (this.hubConnection && (
        this.hubConnection.state === signalR.HubConnectionState.Connected || 
        this.hubConnection.state === signalR.HubConnectionState.Connecting ||
        this.hubConnection.state === signalR.HubConnectionState.Reconnecting
    )) {
      console.log('SignalR subscription channel already open. Skipping initialization loop.');
      return;
    }

    // 1. Dynamically extract base URL of the YARP Gateway and target the notification hub route
    const gatewayBase = environment.apiGatewayUrl.replace(/\/api$/, '');
    
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${gatewayBase}/hub/notifications`)
      .withAutomaticReconnect()
      .build();

    // 2. Start the persistent connection channel
    this.hubConnection
      .start()
      .then(() => console.log('SignalR connection established successfully.'))
      .catch(err => console.error('Error establishing SignalR connection: ', err));

    // 3. Register a listener mapping exactly to the backend broadcast event name and payload structure
    this.hubConnection.on('ReceiveNotification', (notification: { status: string, orderId: string, tracking?: string, transactionId?: string, carrier?: string }) => {
      let mappedStatus = notification.status;
      if (notification.status === 'Order Placed') {
        mappedStatus = 'Processing';
      } else if (notification.status === 'Payment Processed') {
        mappedStatus = 'Payment Approved';
      }

      // Broadcast the change to any component listening in-memory
      this.latestOrderStatusUpdate.set({ 
        orderId: notification.orderId, 
        status: mappedStatus,
        trackingNumber: notification.tracking,
        transactionId: notification.transactionId,
        carrier: notification.carrier
      });

      // Trigger a beautiful notification popup immediately
      this.toastr.info(`Order #${notification.orderId.substring(0, 8)} status updated to: ${mappedStatus}`, 'Real-Time Update');
    });
  }

  public stopConnection(): void {
    // Only stop if it's actually connected
    if (this.hubConnection && this.hubConnection.state === signalR.HubConnectionState.Connected) {
      this.hubConnection.stop()
        .then(() => console.log('SignalR closed safely.'))
        .catch(err => console.error('Error disconnecting cleanly:', err));
    }
  }
}