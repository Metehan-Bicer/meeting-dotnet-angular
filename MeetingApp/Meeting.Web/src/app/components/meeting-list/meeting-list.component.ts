import { Component, OnInit } from '@angular/core';
import { MeetingService } from '../../services/meeting.service';
import { Meeting } from '../../models/meeting';
import { AuthService } from '../../services/auth.service';
import { NotificationService } from '../../shared/services/notification.service';

@Component({
  selector: 'app-meeting-list',
  templateUrl: './meeting-list.component.html',
  styleUrls: ['./meeting-list.component.css']
})
export class MeetingListComponent implements OnInit {
  meetings: Meeting[] = [];
  userId: number = 1; // This should come from the authenticated user

  constructor(
    private meetingService: MeetingService,
    private authService: AuthService,
    private notificationService: NotificationService
  ) {
    // Get user ID from auth service
    this.authService.currentUser.subscribe(user => {
      if (user) {
        this.userId = user.id;
      }
    });
  }

  ngOnInit(): void {
    this.loadMeetings();
  }

  loadMeetings(): void {
    this.meetingService.getUserMeetings(this.userId).subscribe({
      next: (response) => {
        if (response.success) {
          this.meetings = response.data || [];
        } else {
          this.notificationService.showError(response.message || 'Error loading meetings');
        }
      },
      error: (error) => {
        this.notificationService.showError(error);
      }
    });
  }

  cancelMeeting(id: number): void {
    if (confirm('Are you sure you want to cancel this meeting?')) {
      this.meetingService.cancelMeeting(id).subscribe({
        next: (response) => {
          if (response.success) {
            this.notificationService.showSuccess('Meeting cancelled successfully');
            // Reload meetings after cancellation
            this.loadMeetings();
          } else {
            this.notificationService.showError(response.message || 'Error cancelling meeting');
          }
        },
        error: (error) => {
          this.notificationService.showError(error);
        }
      });
    }
  }
}