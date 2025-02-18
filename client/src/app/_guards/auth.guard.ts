import { CanActivateFn } from '@angular/router';
import { AccountService } from '../_services/account.service';
import { inject } from '@angular/core';
import { ToastrService } from 'ngx-toastr';

export const authGuard: CanActivateFn = (route, state) => {
  const toastr = inject(ToastrService);
  const accountService = inject(AccountService);
  
  if (accountService.currentUser()) {
    return true;
  } else {
    toastr.error("You shall not pass!");
    return false;
  }
};
