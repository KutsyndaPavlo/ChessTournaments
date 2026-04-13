import { Component, ChangeDetectionStrategy } from '@angular/core';
import { ReactiveFormsModule, ControlContainer, FormGroupDirective } from '@angular/forms';
import { InputNumberModule } from 'primeng/inputnumber';

@Component({
  selector: 'app-entry-fee-field',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, InputNumberModule],
  viewProviders: [{ provide: ControlContainer, useExisting: FormGroupDirective }],
  template: `
    <fieldset class="flex flex-col">
      <label for="entryFee">Entry Fee <span class="text-red-500">*</span></label>
      <p-inputnumber
        inputId="entryFee"
        formControlName="entryFee"
        mode="currency"
        currency="USD"
        [min]="0"
        placeholder="Enter fee"
        [invalid]="control?.invalid && control?.touched"
        size="small"
      />
      @if (control?.hasError('required') && control?.touched) {
        <small class="p-error">Entry fee is required.</small>
      }
      @if (control?.hasError('greaterThanOrEqual') && control?.touched) {
        <small class="p-error">Entry fee cannot be negative.</small>
      }
    </fieldset>
  `,
})
export class EntryFeeFieldComponent {
  constructor(private controlContainer: ControlContainer) {}

  protected get control() {
    return this.controlContainer.control?.get('entryFee');
  }
}
