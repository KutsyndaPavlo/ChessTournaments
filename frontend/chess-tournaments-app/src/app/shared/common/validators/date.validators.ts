import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import { isAfter, isBefore, isEqual, startOfDay } from 'date-fns';

export class DateValidators {
  /**
   * Validates that the date is not before the specified minimum date.
   */
  static minDate(minDate: Date): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) {
        return null;
      }
      const controlDate = startOfDay(new Date(control.value));
      const min = startOfDay(minDate);
      return isBefore(controlDate, min)
        ? { minDate: { min: minDate, actual: control.value } }
        : null;
    };
  }

  /**
   * Validates that the date is not after the specified maximum date.
   */
  static maxDate(maxDate: Date): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) {
        return null;
      }
      const controlDate = startOfDay(new Date(control.value));
      const max = startOfDay(maxDate);
      return isAfter(controlDate, max)
        ? { maxDate: { max: maxDate, actual: control.value } }
        : null;
    };
  }

  /**
   * Validates that the date is in the future.
   */
  static futureDate(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) {
        return null;
      }
      const controlDate = startOfDay(new Date(control.value));
      const today = startOfDay(new Date());
      return isAfter(controlDate, today) || isEqual(controlDate, today)
        ? null
        : { futureDate: { actual: control.value } };
    };
  }

  /**
   * Validates that the date is in the past.
   */
  static pastDate(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) {
        return null;
      }
      const controlDate = startOfDay(new Date(control.value));
      const today = startOfDay(new Date());
      return isBefore(controlDate, today) ? null : { pastDate: { actual: control.value } };
    };
  }

  /**
   * Validates that the date is within a specified range.
   */
  static dateRange(minDate: Date, maxDate: Date): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) {
        return null;
      }
      const controlDate = startOfDay(new Date(control.value));
      const min = startOfDay(minDate);
      const max = startOfDay(maxDate);

      if (isBefore(controlDate, min) || isAfter(controlDate, max)) {
        return { dateRange: { min: minDate, max: maxDate, actual: control.value } };
      }
      return null;
    };
  }

  /**
   * Validates that one date control is after another date control.
   */
  static dateAfter(startControlName: string, endControlName: string): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const startControl = control.get(startControlName);
      const endControl = control.get(endControlName);

      if (!startControl?.value || !endControl?.value) {
        return null;
      }

      const startDate = startOfDay(new Date(startControl.value));
      const endDate = startOfDay(new Date(endControl.value));

      if (!isAfter(endDate, startDate) && !isEqual(endDate, startDate)) {
        endControl.setErrors({ dateAfter: true });
        return { dateAfter: { start: startControl.value, end: endControl.value } };
      }

      if (endControl.hasError('dateAfter')) {
        const errors = { ...endControl.errors };
        delete errors['dateAfter'];
        endControl.setErrors(Object.keys(errors).length > 0 ? errors : null);
      }

      return null;
    };
  }
}
