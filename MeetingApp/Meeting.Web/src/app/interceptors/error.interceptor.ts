import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { NotificationService } from '../shared/services/notification.service';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  constructor(private notificationService: NotificationService) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(req).pipe(
      catchError((error: HttpErrorResponse) => {
        let errorMessage = 'An unknown error occurred!';
        
        if (error.error instanceof ErrorEvent) {
          // Client-side error
          errorMessage = `Error: ${error.error.message}`;
        } else {
          // Server-side error
          errorMessage = `Error Code: ${error.status}\nMessage: ${error.message}`;
          
          // If we have a response body with our custom error format
          if (error.error && typeof error.error === 'object') {
            if (error.error.message) {
              errorMessage = error.error.message;
            }
            if (error.error.errors && Array.isArray(error.error.errors)) {
              errorMessage = error.error.errors.join(', ');
            }
          }
        }
        
        // Show error notification
        this.notificationService.showError(errorMessage);
        
        return throwError(errorMessage);
      })
    );
  }
}