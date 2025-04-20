import { HttpInterceptorFn } from '@angular/common/http';
import { BusyService } from '../_services/busy.service';
import { delay, finalize, identity } from 'rxjs';
import { inject } from '@angular/core';
import { environment } from '../../environments/environment';

export const loadingInterceptor: HttpInterceptorFn = (req, next) => {

  const busyService = inject(BusyService);
  busyService.busy();

  //The delay is just for showing it works. Remove from PROD
  return next(req).pipe(
    environment.production ? identity : delay(1000),
    finalize(() => {
      busyService.idle();
    })
  )
};
