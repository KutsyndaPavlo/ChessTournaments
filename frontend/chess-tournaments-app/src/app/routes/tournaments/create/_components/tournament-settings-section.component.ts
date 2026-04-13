import { Component, ChangeDetectionStrategy, input } from '@angular/core';
import { ControlContainer, FormGroupDirective } from '@angular/forms';
import {
  FormatFieldComponent,
  TimeControlFieldComponent,
  TimeInMinutesFieldComponent,
  IncrementFieldComponent,
  RoundsFieldComponent,
  EntryFeeFieldComponent,
} from './fields';

interface SelectOption {
  label: string;
  value: string;
}

@Component({
  selector: 'app-tournament-settings-section',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    FormatFieldComponent,
    TimeControlFieldComponent,
    TimeInMinutesFieldComponent,
    IncrementFieldComponent,
    RoundsFieldComponent,
    EntryFeeFieldComponent,
  ],
  viewProviders: [{ provide: ControlContainer, useExisting: FormGroupDirective }],
  template: `
    <div>
      <h2 class="mb-4 text-lg font-semibold">Tournament Settings</h2>
      <div class="grid grid-cols-1 gap-x-6 gap-y-4 md:grid-cols-2 lg:grid-cols-3">
        <app-format-field [options]="formatOptions()" />
        <app-time-control-field [options]="timeControlOptions()" />
        <app-time-in-minutes-field />
        <app-increment-field />
        <app-rounds-field />
        <app-entry-fee-field />
      </div>
    </div>
  `,
})
export class TournamentSettingsSectionComponent {
  formatOptions = input.required<SelectOption[]>();
  timeControlOptions = input.required<SelectOption[]>();
}
