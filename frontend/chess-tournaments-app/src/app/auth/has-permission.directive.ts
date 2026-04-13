import { Directive, Input, TemplateRef, ViewContainerRef, inject, OnInit } from '@angular/core';
import { AuthorizationService, Permission } from './authorization.service';

/**
 * Structural directive to show/hide elements based on user permissions
 * Usage:
 * <button *hasPermission="'create_tournament'">Create Tournament</button>
 * <div *hasPermission="['create_tournament', 'update_tournament']">Manage content</div>
 */
@Directive({
  selector: '[hasPermission]',
  standalone: true,
})
export class HasPermissionDirective implements OnInit {
  private templateRef = inject(TemplateRef<unknown>);
  private viewContainer = inject(ViewContainerRef);
  private authorizationService = inject(AuthorizationService);

  private permissions: Permission[] = [];
  private requireAll = true;

  @Input() set hasPermission(permissions: Permission | Permission[]) {
    this.permissions = Array.isArray(permissions) ? permissions : [permissions];
    this.updateView();
  }

  @Input() set hasPermissionRequireAll(value: boolean) {
    this.requireAll = value;
    this.updateView();
  }

  ngOnInit() {
    this.updateView();
  }

  private updateView() {
    const hasAccess = this.requireAll
      ? this.authorizationService.hasAllPermissions(this.permissions)
      : this.authorizationService.hasAnyPermission(this.permissions);

    if (hasAccess) {
      this.viewContainer.createEmbeddedView(this.templateRef);
    } else {
      this.viewContainer.clear();
    }
  }
}
