import { CanDeactivateFn } from '@angular/router';
import { Observable } from 'rxjs';

/**
 * Interface that components must implement to use the canDeactivate guard.
 */
export interface CanDeactivateComponent {
  canDeactivate(): Observable<boolean> | Promise<boolean> | boolean;
}

/**
 * Guard that prevents navigation away from a component if it has unsaved changes.
 * Components must implement the CanDeactivateComponent interface.
 *
 * @example
 * // In route configuration:
 * {
 *   path: 'create',
 *   component: CreateComponent,
 *   canDeactivate: [canComponentDeactivate],
 * }
 *
 * // In component:
 * export class CreateComponent implements CanDeactivateComponent {
 *   canDeactivate(): Observable<boolean> | boolean {
 *     if (this.form.dirty) {
 *       return this.confirmationService.confirm({
 *         ...CONFIRM_UNSAVED_CHANGES_MESSAGE,
 *       }).pipe(map(result => result === 'accept'));
 *     }
 *     return true;
 *   }
 * }
 */
export const canComponentDeactivate: CanDeactivateFn<CanDeactivateComponent> = (
  component: CanDeactivateComponent,
): Observable<boolean> | Promise<boolean> | boolean => {
  return component.canDeactivate ? component.canDeactivate() : true;
};
