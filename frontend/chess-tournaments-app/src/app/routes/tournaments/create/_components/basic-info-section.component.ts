import { Component, ChangeDetectionStrategy } from '@angular/core';
import { ControlContainer, FormGroupDirective } from '@angular/forms';
import {
  NameFieldComponent,
  LocationFieldComponent,
  DescriptionFieldComponent,
  StartDateFieldComponent,
} from './fields';

@Component({
  selector: 'app-basic-info-section',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    NameFieldComponent,
    LocationFieldComponent,
    DescriptionFieldComponent,
    StartDateFieldComponent,
  ],
  viewProviders: [{ provide: ControlContainer, useExisting: FormGroupDirective }],
  template: `
    <div>
      <h2 class="mb-4 text-lg font-semibold">Basic Information</h2>
      <div class="grid grid-cols-1 gap-x-6 gap-y-4 md:grid-cols-2">
        <app-name-field />
        <app-location-field />
        <app-description-field />
        <app-start-date-field />
      </div>
    </div>
  `,
})
export class BasicInfoSectionComponent {}
