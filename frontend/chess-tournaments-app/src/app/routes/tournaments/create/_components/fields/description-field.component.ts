import { Component, ChangeDetectionStrategy } from '@angular/core';
import { ReactiveFormsModule, ControlContainer, FormGroupDirective } from '@angular/forms';
import { TextareaModule } from 'primeng/textarea';

@Component({
  selector: 'app-description-field',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, TextareaModule],
  viewProviders: [{ provide: ControlContainer, useExisting: FormGroupDirective }],
  template: `
    <fieldset class="col-span-full flex flex-col">
      <label for="description">Description</label>
      <textarea
        pTextarea
        id="description"
        formControlName="description"
        placeholder="Enter tournament description"
        rows="4"
        [class.ng-invalid]="control?.invalid && control?.touched"
      ></textarea>
      @if (control?.hasError('maxlength') && control?.touched) {
        <small class="p-error">Description cannot exceed 2000 characters.</small>
      }
    </fieldset>
  `,
})
export class DescriptionFieldComponent {
  constructor(private controlContainer: ControlContainer) {}

  protected get control() {
    return this.controlContainer.control?.get('description');
  }
}
