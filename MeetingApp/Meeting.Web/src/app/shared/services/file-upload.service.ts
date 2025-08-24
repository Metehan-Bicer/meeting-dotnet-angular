import { Injectable } from '@angular/core';
import { HttpClient, HttpEvent, HttpEventType, HttpRequest } from '@angular/common/http';
import { Observable, Subject } from 'rxjs';
import { map } from 'rxjs/operators';

export interface UploadProgress {
  progress: number;
  state: 'PENDING' | 'IN_PROGRESS' | 'COMPLETED' | 'ERROR';
  response?: any;
  error?: string;
}

@Injectable()
export class FileUploadService {
  constructor(private http: HttpClient) {}

  uploadFile(url: string, file: File, additionalData?: { [key: string]: any }): Observable<UploadProgress> {
    const formData = new FormData();
    
    // Add the file
    formData.append('file', file, file.name);
    
    // Add additional data if provided
    if (additionalData) {
      Object.keys(additionalData).forEach(key => {
        formData.append(key, additionalData[key]);
      });
    }

    const req = new HttpRequest('POST', url, formData, {
      reportProgress: true
    });

    const progressSubject = new Subject<UploadProgress>();

    const subscription = this.http.request(req).subscribe(
      event => {
        if (event.type === HttpEventType.UploadProgress) {
          const progress = Math.round(100 * event.loaded / (event.total || event.loaded));
          progressSubject.next({
            progress,
            state: 'IN_PROGRESS'
          });
        } else if (event.type === HttpEventType.Response) {
          progressSubject.next({
            progress: 100,
            state: 'COMPLETED',
            response: event.body
          });
          progressSubject.complete();
        }
      },
      error => {
        progressSubject.next({
          progress: 0,
          state: 'ERROR',
          error: error.message
        });
        progressSubject.complete();
      }
    );

    // Return observable and subscription for cleanup
    return progressSubject.asObservable();
  }
}