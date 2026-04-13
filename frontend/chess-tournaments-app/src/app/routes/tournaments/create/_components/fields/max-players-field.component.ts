import { Component, ChangeDetectionStrategy } from '@angular/core';
import { ReactiveFormsModule, ControlContainer, FormGroupDirective } from '@angular/forms';
import { InputNumberModule } from 'primeng/inputnumber';

@Component({
  selector: 'app-max-players-field',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, InputNumberModule],
  viewProviders: [{ provide: ControlContainer, useExisting: FormGroupDirective }],
  template: `
    <fieldset class="flex flex-col">
      <label for="maxPlayers">Maximum Players <span class="text-red-500">*</span></label>
      <p-inputnumber
        inputId="maxPlayers"
        formControlName="maxPlayers"
        [min]="2"
        [max]="200"
        placeholder="Enter max players"
        [invalid]="control?.invalid && control?.touched"
        size="small"
      />
      @if (control?.hasError('required') && control?.touched) {
        <small class="p-error">Maximum players is required.</small>
      }
      @if (control?.hasError('range') && control?.touched) {
        <small class="p-error">Max players must be between 2 and 200.</small>
      }
    </fieldset>
  `,
})
export class MaxPlayersFieldComponent {
  constructor(private controlContainer: ControlContainer) {}

  protected get control() {
    return this.controlContainer.control?.get('maxPlayers');
  }
}
