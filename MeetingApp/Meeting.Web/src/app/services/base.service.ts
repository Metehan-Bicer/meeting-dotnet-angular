import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, retry } from 'rxjs/operators';
import { environment } from '../../environments/environment';

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  errors: string[];
  statusCode: number;
}

@Injectable({
  providedIn: 'root'
})
export class BaseService {
  protected apiUrl = environment.apiUrl;

  constructor(protected http: HttpClient) {}

  protected handleError(error: HttpErrorResponse) {
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
    
    return throwError(errorMessage);
  }

  protected get<T>(url: string): Observable<ApiResponse<T>> {
    return this.http.get<ApiResponse<T>>(url)
      .pipe(
        retry(1),
        catchError(this.handleError)
      );
  }

  protected post<T>(url: string, data: any): Observable<ApiResponse<T>> {
    return this.http.post<ApiResponse<T>>(url, data)
      .pipe(
        catchError(this.handleError)
      );
  }

  protected put<T>(url: string, data: any): Observable<ApiResponse<T>> {
    return this.http.put<ApiResponse<T>>(url, data)
      .pipe(
        catchError(this.handleError)
      );
  }

  protected delete<T>(url: string): Observable<ApiResponse<T>> {
    return this.http.delete<ApiResponse<T>>(url)
      .pipe(
        catchError(this.handleError)
      );
  }
}