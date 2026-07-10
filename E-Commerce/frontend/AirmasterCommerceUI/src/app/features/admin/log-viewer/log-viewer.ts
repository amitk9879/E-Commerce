import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SystemLog } from '../../../shared/models/commerce.models';

@Component({
  selector: 'app-log-viewer',
  imports: [CommonModule],
  templateUrl: './log-viewer.html',
  styleUrl: './log-viewer.css'
})
export class LogViewerComponent implements OnInit {
  public systemLogs = signal<SystemLog[]>([]);

  ngOnInit(): void {
    this.systemLogs.set([
      { timestamp: new Date(), level: 'INFORMATION', service: 'YARP.Gateway', correlationId: 'CORR-XYZ-001', message: 'Inbound HTTP POST routed safely to Ordering.API.' },
      { timestamp: new Date(), level: 'INFORMATION', service: 'Ordering.API', correlationId: 'CORR-XYZ-001', message: 'Order record written to database transaction outbox.' },
      { timestamp: new Date(), level: 'WARNING', service: 'CloudAMQP.Broker', correlationId: 'CORR-XYZ-001', message: 'Delivery channel re-establishing link handshake.' },
      { timestamp: new Date(), level: 'EXCEPTION', service: 'Payment.Worker', correlationId: 'CORR-XYZ-999', message: 'Timeout calling external merchant gateway. Retrying transaction execution sequence.' }
    ]);
  }
}