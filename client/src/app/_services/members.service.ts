import { HttpClient, HttpParams, HttpResponse } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { Member } from '../_models/member';
import { Photo } from '../_models/photo';
import { PaginatedResult } from '../_models/pagination';
import { UserParams } from '../_models/userParams';
import { of } from 'rxjs';
import { AccountService } from './account.service';

@Injectable({
  providedIn: 'root'
})
export class MembersService {
  private accountService = inject(AccountService);
  private http = inject(HttpClient)
  baseUrl = environment.apiUrl;
  paginatedResult = signal<PaginatedResult<Member[]> | null>(null);
  memberCache = new Map();
  user = this.accountService.currentUser();
  userParams = signal<UserParams>(new UserParams(this.user));

  
  resetUserParams() {
    this.userParams.set(new UserParams(this.user));
  }

  getMembers() {
    
    //Check the cache
    const response = this.memberCache.get(Object.values(this.userParams()).join('-'));
    if (response) return this.setPaginatedResponse(response);


    let params = this.setPaginationHeaders(this.userParams().pageNumber, this.userParams().pageSize);

    params = params
      .append('minAge', this.userParams().minAge)
      .append('maxAge', this.userParams().maxAge)
      .append('gender', this.userParams().gender)
      .append('orderBy', this.userParams().orderBy);

    //Process the Http response 
    return this.http.get<Member[]>(this.baseUrl + "users", {observe: 'response', params}).subscribe({
      next: response => {
        this.setPaginatedResponse(response);
        this.memberCache.set(Object.values(this.userParams()).join('-'), response);
      }
    });
  }

  getMember(username: string) {
    const member: Member = [...this.memberCache.values()]
      .reduce((arr, elem) => arr.concat(elem.body), [])
      .find((m: Member) => m.username === username);

    // ensure return as Observable
    if (member) return of(member);

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

  private setPaginationHeaders(pageNumber: number, pageSize: number) {
    //Set up the Http request
    let params = new HttpParams();
    
    if(pageNumber && pageSize) {
      params = params
        .append('pageNumber', pageNumber)
        .append('pageSize', pageSize)
    }

    return params;
  }

  private setPaginatedResponse(response: HttpResponse<Member[]>) {
    this.paginatedResult.set({
      items: response.body as Member[],
      pagination: JSON.parse(response.headers.get('Pagination')!)
    })

  }
}
