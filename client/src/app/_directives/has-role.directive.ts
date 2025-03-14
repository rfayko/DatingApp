import { Directive, inject, Input, input, OnInit, TemplateRef, ViewContainerRef } from '@angular/core';
import { AccountService } from '../_services/account.service';

@Directive({
  selector: '[appHasRole]',  // *appHasRole has to be used, asterisk is required.
  standalone: true
})
export class HasRoleDirective implements OnInit{
  @Input() appHasRole: string[] = [];  // Neil says does not have to be a signal? So use old @Input? Why?
  private accountService = inject(AccountService);
  private viewContainerRef = inject(ViewContainerRef);
  private templateRef = inject(TemplateRef);


  ngOnInit(): void {
    if (this.accountService.roles()?.some((r: string) => this.appHasRole.includes(r))) {
      this.viewContainerRef.createEmbeddedView(this.templateRef);
    } else {
      this.viewContainerRef.clear();
    }
  }


}
