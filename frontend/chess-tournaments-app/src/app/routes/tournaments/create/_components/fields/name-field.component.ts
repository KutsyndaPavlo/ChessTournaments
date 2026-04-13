import { Component, ChangeDetectionStrategy } from '@angular/core';
import { ReactiveFormsModule, ControlContainer, FormGroupDirective } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';

@Component({
  selector: 'app-name-field',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, InputTextModule],
  viewProviders: [{ provide: ControlContainer, useExisting: FormGroupDirective }],
  template: `
    <fieldset class="flex flex-col">
      <label for="name">Tournament Name <span class="text-red-500">*</span></label>
      <input
        pInputText
        id="name"
        formControlName="name"
        placeholder="Enter tournament name"
        [class.ng-invalid]="control?.invalid && control?.touched"
        size="small"
      />
      @if (control?.hasError('required') && control?.touched) {
        <small class="p-error">Tournament name is required.</small>
      }
      @if (control?.hasError('maxlength') && control?.touched) {
        <small class="p-error">Name cannot exceed 200 characters.</small>
      }
      @if (control?.hasError('notOnlyWhitespace') && control?.touched) {
        <small class="p-error">Name cannot be only whitespace.</small>
      }
    </fieldset>
  `,
})
export class NameFieldComponent {
  constructor(private controlContainer: ControlContainer) {}

  protected get control() {
    return this.controlContainer.control?.get('name');
  }
}
