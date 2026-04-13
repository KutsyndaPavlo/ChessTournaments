import { Component, ChangeDetectionStrategy, input } from '@angular/core';
import { ReactiveFormsModule, ControlContainer, FormGroupDirective } from '@angular/forms';
import { SelectModule } from 'primeng/select';

interface SelectOption {
  label: string;
  value: string;
}

@Component({
  selector: 'app-format-field',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, SelectModule],
  viewProviders: [{ provide: ControlContainer, useExisting: FormGroupDirective }],
  template: `
    <fieldset class="flex flex-col">
      <label for="format">Format <span class="text-red-500">*</span></label>
      <p-select
        inputId="format"
        formControlName="format"
        [options]="options()"
        placeholder="Select format"
        optionLabel="label"
        optionValue="value"
        [invalid]="control?.invalid && control?.touched"
        size="small"
      />
      @if (control?.hasError('required') && control?.touched) {
        <small class="p-error">Please select a format.</small>
      }
    </fieldset>
  `,
})
export class FormatFieldComponent {
  options = input.required<SelectOption[]>();

  constructor(private controlContainer: ControlContainer) {}

  protected get control() {
    return this.controlContainer.control?.get('format');
  }
}
