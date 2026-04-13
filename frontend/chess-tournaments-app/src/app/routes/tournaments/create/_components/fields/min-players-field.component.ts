import { Component, ChangeDetectionStrategy } from '@angular/core';
import { ReactiveFormsModule, ControlContainer, FormGroupDirective } from '@angular/forms';
import { InputNumberModule } from 'primeng/inputnumber';

@Component({
  selector: 'app-min-players-field',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, InputNumberModule],
  viewProviders: [{ provide: ControlContainer, useExisting: FormGroupDirective }],
  template: `
    <fieldset class="flex flex-col">
      <label for="minPlayers">Minimum Players <span class="text-red-500">*</span></label>
      <p-inputnumber
        inputId="minPlayers"
        formControlName="minPlayers"
        [min]="2"
        [max]="200"
        placeholder="Enter min players"
        [invalid]="control?.invalid && control?.touched"
        size="small"
      />
      @if (control?.hasError('required') && control?.touched) {
        <small class="p-error">Minimum players is required.</small>
      }
      @if (control?.hasError('range') && control?.touched) {
        <small class="p-error">Min players must be between 2 and 200.</small>
      }
    </fieldset>
  `,
})
export class MinPlayersFieldComponent {
  constructor(private controlContainer: ControlContainer) {}

  protected get control() {
    return this.controlContainer.control?.get('minPlayers');
  }
}
