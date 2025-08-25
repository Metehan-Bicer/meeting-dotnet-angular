import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MeetingService } from '../../services/meeting.service';
import { Meeting } from '../../models/meeting';
import { NotificationService } from '../../shared/services/notification.service';

@Component({
  selector: 'app-meeting-detail',
  templateUrl: './meeting-detail.component.html',
  styleUrls: ['./meeting-detail.component.css']
})
export class MeetingDetailComponent implements OnInit {
  meeting: Meeting | null = null;
  loading = true;

  constructor(
    private route: ActivatedRoute,
    public router: Router,
    private meetingService: MeetingService,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadMeeting(+id);
    }
  }

  loadMeeting(id: number): void {
    this.loading = true;
    this.meetingService.getMeetingById(id).subscribe({
      next: (response) => {
        this.loading = false;
        if (response.success && response.data) {
          this.meeting = response.data;
        } else {
          this.notificationService.showError(response.message || 'Error loading meeting');
          this.router.navigate(['/meetings']);
        }
      },
      error: (error) => {
        this.loading = false;
        this.notificationService.showError('Error loading meeting');
        this.router.navigate(['/meetings']);
      }
    });
  }

  cancelMeeting(): void {
    if (this.meeting && confirm('Are you sure you want to cancel this meeting?')) {
      this.meetingService.cancelMeeting(this.meeting.id).subscribe({
        next: (response) => {
          if (response.success) {
            this.notificationService.showSuccess('Meeting cancelled successfully');
            this.router.navigate(['/meetings']);
          } else {
            this.notificationService.showError(response.message || 'Failed to cancel meeting');
          }
        },
        error: (error) => {
          this.notificationService.showError(error);
        }
      });
    }
  }

  navigateBack(): void {
    this.router.navigate(['/meetings']);
  }

  getDocumentName(path: string | null | undefined): string {
    if (!path) return 'Unknown file';
    const pathParts = path.split('/');
    return pathParts[pathParts.length - 1];
  }
}