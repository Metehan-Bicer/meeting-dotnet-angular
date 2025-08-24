import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { NotificationService } from '../../shared/services/notification.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent {
  registerForm: FormGroup;
  profileImage: File | null = null;
  loading = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private notificationService: NotificationService
  ) {
    this.registerForm = this.fb.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      phoneNumber: ['', Validators.required],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required]
    });
  }

  onFileSelected(event: any) {
    this.profileImage = event.target.files[0];
  }

  onSubmit() {
    if (this.registerForm.valid) {
      this.loading = true;
      
      const formData = new FormData();
      formData.append('firstName', this.registerForm.get('firstName')?.value);
      formData.append('lastName', this.registerForm.get('lastName')?.value);
      formData.append('email', this.registerForm.get('email')?.value);
      formData.append('phoneNumber', this.registerForm.get('phoneNumber')?.value);
      formData.append('password', this.registerForm.get('password')?.value);
      formData.append('confirmPassword', this.registerForm.get('confirmPassword')?.value);
      
      if (this.profileImage) {
        formData.append('profileImage', this.profileImage);
      }

      this.authService.register(formData).subscribe({
        next: (response: any) => {
          this.loading = false;
          if (response.success) {
            this.notificationService.showSuccess('Registration successful! Please login.');
            this.router.navigate(['/login']);
          } else {
            this.notificationService.showError(response.message || 'Registration failed');
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