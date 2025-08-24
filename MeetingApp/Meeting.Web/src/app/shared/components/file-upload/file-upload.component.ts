import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FileUploadService, UploadProgress } from '../../services/file-upload.service';

@Component({
  selector: 'app-file-upload',
  templateUrl: './file-upload.component.html',
  styleUrls: ['./file-upload.component.css']
})
export class FileUploadComponent {
  @Input() accept: string = '*';
  @Input() multiple: boolean = false;
  @Input() uploadUrl: string = '';
  @Input() additionalData: { [key: string]: any } = {};
  
  @Output() fileSelected = new EventEmitter<File | File[]>();
  @Output() uploadProgress = new EventEmitter<UploadProgress>();
  @Output() uploadComplete = new EventEmitter<any>();
  @Output() uploadError = new EventEmitter<string>();

  selectedFiles: File[] = [];
  isUploading = false;
  uploadProgressData: UploadProgress | null = null;

  constructor(private fileUploadService: FileUploadService) {}

  onFileChange(event: any) {
    const files: FileList = event.target.files;
    
    if (files && files.length > 0) {
      this.selectedFiles = Array.from(files);
      
      if (this.multiple) {
        this.fileSelected.emit(this.selectedFiles);
      } else {
        this.fileSelected.emit(this.selectedFiles[0]);
      }
    }
  }

  uploadFiles() {
    if (this.selectedFiles.length === 0 || !this.uploadUrl) {
      return;
    }

    this.isUploading = true;
    this.uploadProgressData = null;

    // For simplicity, we'll upload the first file
    // In a real application, you might want to upload all files
    const fileToUpload = this.selectedFiles[0];
    
    this.fileUploadService.uploadFile(this.uploadUrl, fileToUpload, this.additionalData)
      .subscribe({
        next: (progress) => {
          this.uploadProgressData = progress;
          this.uploadProgress.emit(progress);
          
          if (progress.state === 'COMPLETED') {
            this.isUploading = false;
            this.uploadComplete.emit(progress.response);
          }
        },
        error: (error) => {
          this.isUploading = false;
          this.uploadProgressData = {
            progress: 0,
            state: 'ERROR',
            error: error
          };
          this.uploadError.emit(error);
        }
      });
  }

  clearFiles() {
    this.selectedFiles = [];
    this.uploadProgressData = null;
    this.isUploading = false;
  }
}