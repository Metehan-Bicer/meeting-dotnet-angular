import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

export interface Notification {
  id: number;
  type: 'success' | 'error' | 'warning' | 'info';
  message: string;
  duration?: number; // in milliseconds
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private notificationsSubject = new BehaviorSubject<Notification[]>([]);
  public notifications$ = this.notificationsSubject.asObservable();
  private idCounter = 0;

  showSuccess(message: string, duration: number = 5000): void {
    this.addNotification('success', message, duration);
  }

  showError(message: string, duration: number = 5000): void {
    this.addNotification('error', message, duration);
  }

  showWarning(message: string, duration: number = 5000): void {
    this.addNotification('warning', message, duration);
  }

  showInfo(message: string, duration: number = 5000): void {
    this.addNotification('info', message, duration);
  }

  removeNotification(id: number): void {
    const currentNotifications = this.notificationsSubject.value;
    const updatedNotifications = currentNotifications.filter(n => n.id !== id);
    this.notificationsSubject.next(updatedNotifications);
  }

  clearAll(): void {
    this.notificationsSubject.next([]);
  }

  private addNotification(type: 'success' | 'error' | 'warning' | 'info', message: string, duration: number): void {
    const id = this.idCounter++;
    const notification: Notification = { id, type, message, duration };
    
    const currentNotifications = this.notificationsSubject.value;
    this.notificationsSubject.next([...currentNotifications, notification]);
    
    // Auto remove after duration
    if (duration > 0) {
      setTimeout(() => {
        this.removeNotification(id);
      }, duration);
    }
  }
}