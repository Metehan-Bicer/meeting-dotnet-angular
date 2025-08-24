import { Component, OnInit } from '@angular/core';
import { Notification, NotificationService } from '../../services/notification.service';

@Component({
  selector: 'app-notification',
  templateUrl: './notification.component.html',
  styleUrls: ['./notification.component.css']
})
export class NotificationComponent implements OnInit {
  notifications: Notification[] = [];

  constructor(private notificationService: NotificationService) {}

  ngOnInit(): void {
    this.notificationService.notifications$.subscribe(notifications => {
      this.notifications = notifications;
    });
  }

  removeNotification(id: number): void {
    this.notificationService.removeNotification(id);
  }

  getIconClass(type: string): string {
    switch (type) {
      case 'success': return 'bi bi-check-circle-fill';
      case 'error': return 'bi bi-exclamation-triangle-fill';
      case 'warning': return 'bi bi-exclamation-circle-fill';
      case 'info': return 'bi bi-info-circle-fill';
      default: return '';
    }
  }

  getAlertClass(type: string): string {
    switch (type) {
      case 'success': return 'alert-success';
      case 'error': return 'alert-danger';
      case 'warning': return 'alert-warning';
      case 'info': return 'alert-info';
      default: return 'alert-info';
    }
  }
}