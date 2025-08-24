import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { LoadingComponent } from './components/loading/loading.component';
import { NotificationComponent } from './components/notification/notification.component';
import { FileUploadComponent } from './components/file-upload/file-upload.component';
import { DateFormatPipe } from './pipes/date-format.pipe';
import { FileSizePipe } from './pipes/file-size.pipe';
import { PasswordMatchDirective } from './directives/password-match.directive';
import { FileUploadService } from './services/file-upload.service';

@NgModule({
  declarations: [
    LoadingComponent,
    NotificationComponent,
    FileUploadComponent,
    DateFormatPipe,
    FileSizePipe,
    PasswordMatchDirective
  ],
  imports: [
    CommonModule,
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule
  ],
  providers: [
    FileUploadService
  ],
  exports: [
    CommonModule,
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
    LoadingComponent,
    NotificationComponent,
    FileUploadComponent,
    DateFormatPipe,
    FileSizePipe,
    PasswordMatchDirective
  ]
})
export class SharedModule { }