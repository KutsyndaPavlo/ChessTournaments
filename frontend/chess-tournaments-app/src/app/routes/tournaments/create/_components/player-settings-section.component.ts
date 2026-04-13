import { Component, ChangeDetectionStrategy } from '@angular/core';
import { ControlContainer, FormGroupDirective } from '@angular/forms';
import {
  MinPlayersFieldComponent,
  MaxPlayersFieldComponent,
  AllowByesFieldComponent,
} from './fields';

@Component({
  selector: 'app-player-settings-section',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [MinPlayersFieldComponent, MaxPlayersFieldComponent, AllowByesFieldComponent],
  viewProviders: [{ provide: ControlContainer, useExisting: FormGroupDirective }],
  template: `
    <div>
      <h2 class="mb-4 text-lg font-semibold">Player Settings</h2>
      <div class="grid grid-cols-1 gap-x-6 gap-y-4 md:grid-cols-2 lg:grid-cols-3">
        <app-min-players-field />
        <app-max-players-field />
        <app-allow-byes-field />
      </div>
    </div>
  `,
})
export class PlayerSettingsSectionComponent {}
