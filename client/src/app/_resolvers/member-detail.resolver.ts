import { ActivatedRoute, ResolveFn } from '@angular/router';
import { Member } from '../_models/member';
import { MembersService } from '../_services/members.service';
import { inject } from '@angular/core';

export const memberDetailResolver: ResolveFn<Member | null> = (route, state) => {
  const memberService = inject(MembersService);

  var userName = route.paramMap.get('username');
  if (userName == null) return null;

  return memberService.getMember(userName);  // Note that we do not subscribe as Angular resolvers handle subscriptions
}


// Resolvers get data before the route is activated so that said data is available to component prior to ctor.