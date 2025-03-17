import { HttpInterceptorFn } from '@angular/common/http';
import { BusyService } from '../_services/busy.service';
import { delay, finalize } from 'rxjs';
import { inject } from '@angular/core';

export const loadingInterceptor: HttpInterceptorFn = (req, next) => {

  const busyService = inject(BusyService);
  busyService.busy();

  //The delay is just for showing it works. Remove from PROD
  return next(req).pipe(
    delay(1000),
    finalize(() => {
      busyService.idle();
    })
  )
};
