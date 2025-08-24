import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { User } from '../models/user';
import { LoginResponse } from '../models/login-response';
import { NotificationService } from '../shared/services/notification.service';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = environment.apiUrl;
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser = this.currentUserSubject.asObservable();

  constructor(
    private http: HttpClient,
    private notificationService: NotificationService
  ) {
    // Check if user is already logged in
    const user = localStorage.getItem('user');
    if (user) {
      this.currentUserSubject.next(JSON.parse(user));
    }
  }

  register(formData: FormData): Observable<any> {
    return this.http.post(`${this.apiUrl}/auth/register`, formData)
      .pipe(
        tap(() => {
          this.notificationService.showSuccess('Registration successful! Please login.');
        })
      );
  }

  login(credentials: any): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/auth/login`, credentials)
      .pipe(
        tap(response => {
          if (response.success && response.data) {
            localStorage.setItem('token', response.data.token);
            localStorage.setItem('user', JSON.stringify({
              id: response.data.userId,
              email: response.data.email
            }));
            this.currentUserSubject.next({
              id: response.data.userId,
              email: response.data.email
            } as User);
            this.notificationService.showSuccess('Login successful!');
          }
        })
      );
  }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    this.currentUserSubject.next(null);
    this.notificationService.showInfo('You have been logged out.');
  }

  isAuthenticated(): boolean {
    const token = localStorage.getItem('token');
    return !!token;
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }
}