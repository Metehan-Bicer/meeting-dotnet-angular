import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Meeting } from '../models/meeting';
import { BaseService, ApiResponse } from './base.service';
import { NotificationService } from '../shared/services/notification.service';

@Injectable({
  providedIn: 'root'
})
export class MeetingService extends BaseService {
  private meetingApiUrl = `${this.apiUrl}/meetings`;

  constructor(
    http: HttpClient,
    private notificationService: NotificationService
  ) {
    super(http);
  }

  createMeeting(formData: FormData): Observable<ApiResponse<any>> {
    return this.post<any>(this.meetingApiUrl, formData);
  }

  updateMeeting(id: number, formData: FormData): Observable<ApiResponse<any>> {
    return this.put<any>(`${this.meetingApiUrl}/${id}`, formData);
  }

  cancelMeeting(id: number): Observable<ApiResponse<any>> {
    return this.delete<any>(`${this.meetingApiUrl}/${id}/cancel`);
  }

  getAllMeetings(): Observable<ApiResponse<Meeting[]>> {
    return this.get<Meeting[]>(this.meetingApiUrl);
  }

  getUserMeetings(userId: number): Observable<ApiResponse<Meeting[]>> {
    return this.get<Meeting[]>(`${this.meetingApiUrl}/user/${userId}`);
  }
}