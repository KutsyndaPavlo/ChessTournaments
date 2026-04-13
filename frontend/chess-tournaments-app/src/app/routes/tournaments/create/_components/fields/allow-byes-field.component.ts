import { Component, ChangeDetectionStrategy } from '@angular/core';
import { ReactiveFormsModule, ControlContainer, FormGroupDirective } from '@angular/forms';
import { CheckboxModule } from 'primeng/checkbox';

@Component({
  selector: 'app-allow-byes-field',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, CheckboxModule],
  viewProviders: [{ provide: ControlContainer, useExisting: FormGroupDirective }],
  template: `
    <fieldset class="flex flex-col justify-end">
      <div class="flex items-center gap-2">
        <p-checkbox formControlName="allowByes" inputId="allowByes" [binary]="true" />
        <label for="allowByes">Allow bye rounds</label>
      </div>
    </fieldset>
  `,
})
export class AllowByesFieldComponent {
  constructor(private controlContainer: ControlContainer) {}
}
