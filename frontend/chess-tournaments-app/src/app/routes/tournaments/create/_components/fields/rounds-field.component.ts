import { Component, ChangeDetectionStrategy } from '@angular/core';
import { ReactiveFormsModule, ControlContainer, FormGroupDirective } from '@angular/forms';
import { InputNumberModule } from 'primeng/inputnumber';

@Component({
  selector: 'app-rounds-field',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, InputNumberModule],
  viewProviders: [{ provide: ControlContainer, useExisting: FormGroupDirective }],
  template: `
    <fieldset class="flex flex-col">
      <label for="numberOfRounds">Number of Rounds <span class="text-red-500">*</span></label>
      <p-inputnumber
        inputId="numberOfRounds"
        formControlName="numberOfRounds"
        [min]="1"
        [max]="20"
        placeholder="Enter rounds"
        [invalid]="control?.invalid && control?.touched"
        size="small"
      />
      @if (control?.hasError('required') && control?.touched) {
        <small class="p-error">Number of rounds is required.</small>
      }
      @if (control?.hasError('range') && control?.touched) {
        <small class="p-error">Rounds must be between 1 and 20.</small>
      }
    </fieldset>
  `,
})
export class RoundsFieldComponent {
  constructor(private controlContainer: ControlContainer) {}

  protected get control() {
    return this.controlContainer.control?.get('numberOfRounds');
  }
}
