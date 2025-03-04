import { HttpParams, HttpResponse } from "@angular/common/http";
import { PaginatedResult } from "../_models/pagination";
import { signal } from "@angular/core";

export function setPaginatedResponse<T>(response: HttpResponse<T>,
  paginatedResultSignal: ReturnType<typeof signal<PaginatedResult<T> | null>>) {
    paginatedResultSignal.set({
      items: response.body as T,
      pagination: JSON.parse(response.headers.get('Pagination')!)
    })
}


export function setPaginationHeaders(pageNumber: number, pageSize: number) {
  //Set up the Http request
  let params = new HttpParams();
  
  if(pageNumber && pageSize) {
    params = params
      .append('pageNumber', pageNumber)
      .append('pageSize', pageSize)
  }

  return params;
}