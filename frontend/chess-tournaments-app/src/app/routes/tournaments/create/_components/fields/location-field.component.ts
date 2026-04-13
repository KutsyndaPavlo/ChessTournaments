import { Component, ChangeDetectionStrategy } from '@angular/core';
import { ReactiveFormsModule, ControlContainer, FormGroupDirective } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';

@Component({
  selector: 'app-location-field',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, InputTextModule],
  viewProviders: [{ provide: ControlContainer, useExisting: FormGroupDirective }],
  template: `
    <fieldset class="flex flex-col">
      <label for="location">Location</label>
      <input
        pInputText
        id="location"
        formControlName="location"
        placeholder="Enter location"
        [class.ng-invalid]="control?.invalid && control?.touched"
        size="small"
      />
      @if (control?.hasError('maxlength') && control?.touched) {
        <small class="p-error">Location cannot exceed 200 characters.</small>
      }
    </fieldset>
  `,
})
export class LocationFieldComponent {
  constructor(private controlContainer: ControlContainer) {}

  protected get control() {
    return this.controlContainer.control?.get('location');
  }
}
