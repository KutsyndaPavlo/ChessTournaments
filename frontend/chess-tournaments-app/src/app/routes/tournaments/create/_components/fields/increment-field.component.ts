import { Component, ChangeDetectionStrategy } from '@angular/core';
import { ReactiveFormsModule, ControlContainer, FormGroupDirective } from '@angular/forms';
import { InputNumberModule } from 'primeng/inputnumber';

@Component({
  selector: 'app-increment-field',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, InputNumberModule],
  viewProviders: [{ provide: ControlContainer, useExisting: FormGroupDirective }],
  template: `
    <fieldset class="flex flex-col">
      <label for="incrementInSeconds"
        >Increment (seconds) <span class="text-red-500">*</span></label
      >
      <p-inputnumber
        inputId="incrementInSeconds"
        formControlName="incrementInSeconds"
        [min]="0"
        [max]="60"
        placeholder="Enter increment"
        [invalid]="control?.invalid && control?.touched"
        size="small"
      />
      @if (control?.hasError('required') && control?.touched) {
        <small class="p-error">Increment is required.</small>
      }
      @if (control?.hasError('range') && control?.touched) {
        <small class="p-error">Increment must be between 0 and 60 seconds.</small>
      }
    </fieldset>
  `,
})
export class IncrementFieldComponent {
  constructor(private controlContainer: ControlContainer) {}

  protected get control() {
    return this.controlContainer.control?.get('incrementInSeconds');
  }
}
