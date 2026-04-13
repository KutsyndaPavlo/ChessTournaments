import { Component, ChangeDetectionStrategy } from '@angular/core';
import { ReactiveFormsModule, ControlContainer, FormGroupDirective } from '@angular/forms';
import { InputNumberModule } from 'primeng/inputnumber';

@Component({
  selector: 'app-time-in-minutes-field',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, InputNumberModule],
  viewProviders: [{ provide: ControlContainer, useExisting: FormGroupDirective }],
  template: `
    <fieldset class="flex flex-col">
      <label for="timeInMinutes">Time (minutes) <span class="text-red-500">*</span></label>
      <p-inputnumber
        inputId="timeInMinutes"
        formControlName="timeInMinutes"
        [min]="1"
        [max]="240"
        placeholder="Enter time"
        [invalid]="control?.invalid && control?.touched"
        size="small"
      />
      @if (control?.hasError('required') && control?.touched) {
        <small class="p-error">Time is required.</small>
      }
      @if (control?.hasError('range') && control?.touched) {
        <small class="p-error">Time must be between 1 and 240 minutes.</small>
      }
    </fieldset>
  `,
})
export class TimeInMinutesFieldComponent {
  constructor(private controlContainer: ControlContainer) {}

  protected get control() {
    return this.controlContainer.control?.get('timeInMinutes');
  }
}
