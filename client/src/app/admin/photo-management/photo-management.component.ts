import { Component, inject, OnInit, signal } from '@angular/core';
import { Photo, PhotoToModerate } from '../../_models/photo';
import { AdminService } from '../../_services/admin.service';
import { NgClass, NgFor, NgIf, NgStyle } from '@angular/common';

@Component({
  selector: 'app-photo-management',
  standalone: true,
  imports: [],
  templateUrl: './photo-management.component.html',
  styleUrl: './photo-management.component.css'
})
export class PhotoManagementComponent  implements OnInit{
  adminService = inject(AdminService);
  photos: PhotoToModerate[] = [];
  

  ngOnInit(): void {
    this.getUnapprovedPhotos();
  }

  getUnapprovedPhotos() {
    this.adminService.getUnapprovedPhotos().subscribe({
      next: photos => this.photos = photos
    });
  }

  approvePhoto(photoId: number) {
    this.adminService.approvePhoto(photoId).subscribe({
      next: () => this.photos.splice(this.photos.findIndex(p => p.id === photoId), 1)
    });
  }

  rejectPhoto(photoId: number) {
    this.adminService.rejectPhoto(photoId).subscribe({
      next: () => this.photos.splice(this.photos.findIndex(p => p.id === photoId), 1)
    });
  }

}
