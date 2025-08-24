import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { MeetingService } from '../../services/meeting.service';
import { Meeting } from '../../models/meeting';
import { NotificationService } from '../../shared/services/notification.service';

@Component({
  selector: 'app-meeting-create',
  templateUrl: './meeting-create.component.html',
  styleUrls: ['./meeting-create.component.css']
})
export class MeetingCreateComponent implements OnInit {
  meetingForm: FormGroup;
  document: File | null = null;
  isEditMode = false;
  meetingId: number | null = null;
  loading = false;

  constructor(
    private fb: FormBuilder,
    private meetingService: MeetingService,
    private router: Router,
    private route: ActivatedRoute,
    private notificationService: NotificationService
  ) {
    this.meetingForm = this.fb.group({
      title: ['', Validators.required],
      description: [''],
      startDate: ['', Validators.required],
      endDate: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    // Check if we're in edit mode
    this.route.params.subscribe(params => {
      if (params['id']) {
        this.isEditMode = true;
        this.meetingId = +params['id'];
        // In a real app, you would load the meeting data here
        // this.loadMeeting(this.meetingId);
      }
    });
  }

  onFileSelected(event: any) {
    this.document = event.target.files[0];
  }

  onSubmit() {
    if (this.meetingForm.valid) {
      this.loading = true;
      
      const formData = new FormData();
      formData.append('title', this.meetingForm.get('title')?.value);
      formData.append('description', this.meetingForm.get('description')?.value);
      formData.append('startDate', new Date(this.meetingForm.get('startDate')?.value).toISOString());
      formData.append('endDate', new Date(this.meetingForm.get('endDate')?.value).toISOString());
      
      if (this.document) {
        formData.append('document', this.document);
      }

      if (this.isEditMode && this.meetingId) {
        // Update existing meeting
        this.meetingService.updateMeeting(this.meetingId, formData).subscribe({
          next: (response) => {
            this.loading = false;
            if (response.success) {
              this.notificationService.showSuccess('Meeting updated successfully');
              this.router.navigate(['/meetings']);
            } else {
              this.notificationService.showError(response.message || 'Error updating meeting');
            }
          },
          error: (error) => {
            this.loading = false;
            this.notificationService.showError(error);
          }
        });
      } else {
        // Create new meeting
        this.meetingService.createMeeting(formData).subscribe({
          next: (response) => {
            this.loading = false;
            if (response.success) {
              this.notificationService.showSuccess('Meeting created successfully');
              this.router.navigate(['/meetings']);
            } else {
              this.notificationService.showError(response.message || 'Error creating meeting');
            }
          },
          error: (error) => {
            this.loading = false;
            this.notificationService.showError(error);
          }
        });
      }
    }
  }
}