import { Directive, Input, TemplateRef, ViewContainerRef, inject, OnInit } from '@angular/core';
import { AuthorizationService, Role } from './authorization.service';

/**
 * Structural directive to show/hide elements based on user roles
 * Usage:
 * <div *hasRole="'Admin'">Admin only content</div>
 * <div *hasRole="['Admin', 'Organizer']">Admin or Organizer content</div>
 */
@Directive({
  selector: '[hasRole]',
  standalone: true,
})
export class HasRoleDirective implements OnInit {
  private templateRef = inject(TemplateRef<unknown>);
  private viewContainer = inject(ViewContainerRef);
  private authorizationService = inject(AuthorizationService);

  private roles: Role[] = [];

  @Input() set hasRole(roles: Role | Role[]) {
    this.roles = Array.isArray(roles) ? roles : [roles];
    this.updateView();
  }

  ngOnInit() {
    this.updateView();
  }

  private updateView() {
    if (this.authorizationService.hasAnyRole(this.roles)) {
      this.viewContainer.createEmbeddedView(this.templateRef);
    } else {
      this.viewContainer.clear();
    }
  }
}
