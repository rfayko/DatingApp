import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { Member } from '../_models/member';
import { Photo } from '../_models/photo';
import { PaginatedResult } from '../_models/pagination';

@Injectable({
  providedIn: 'root'
})
export class MembersService {

  private http = inject(HttpClient)
  baseUrl = environment.apiUrl;
  paginatedResult = signal<PaginatedResult<Member[]> | null>(null);

  getMembers(pageNumber?: number, pageSize?: number) {

    //Set up the Http request
    let params = new HttpParams();

    if(pageNumber && pageSize) {
      params = params.append('pageNumber', pageNumber).append('pageSize', pageSize);
    }

    //Process the Http response 
    return this.http.get<Member[]>(this.baseUrl + "users", {observe: 'response', params}).subscribe({
      next: response => {
        this.paginatedResult.set({
          items: response.body as Member[],
          pagination: JSON.parse(response.headers.get('Pagination')!)
        })
      }
    })
  }

  getMember(username: string) {
    // const member = this.members().find(m => m.username == username);
    // if(member !== undefined) return of(member);

    return this.http.get<Member>(this.baseUrl + "users/" + username);
  }

  updateMember(member: Member) {
    return this.http.put(this.baseUrl + 'users', member).pipe(
      // tap(() => {
      //   this.members.update(members => members.map(m => m.username === member.username ? member : m));
      // })
    );
  }

  deletePhoto(photo: Photo) {
    return this.http.delete(this.baseUrl + "users/delete-photo/" + photo.id, {});
  }

  setMainPhoto(photo: Photo) {
    return this.http.put(this.baseUrl + "users/set-main-photo/" + photo.id, {}).pipe(
      // tap(() => {
      //   this.members.update(members => members.map(m => {
      //     if (m.photos.includes(photo)) {
      //       m.photoUrl = photo.url;       //member.photoUrl is the MAIN photo url
      //     }
      //     return m;
      //   }))
      // })
    );
  }
}
