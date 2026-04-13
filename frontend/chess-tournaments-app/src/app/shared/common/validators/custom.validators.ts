import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export class CustomValidators {
  /**
   * Validates that the control value is greater than the specified minimum.
   */
  static greaterThan(min: number): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (control.value === null || control.value === undefined || control.value === '') {
        return null;
      }
      const value = Number(control.value);
      return value > min ? null : { greaterThan: { min, actual: value } };
    };
  }

  /**
   * Validates that the control value is greater than or equal to the specified minimum.
   */
  static greaterThanOrEqual(min: number): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (control.value === null || control.value === undefined || control.value === '') {
        return null;
      }
      const value = Number(control.value);
      return value >= min ? null : { greaterThanOrEqual: { min, actual: value } };
    };
  }

  /**
   * Validates that the control value is less than the specified maximum.
   */
  static lessThan(max: number): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (control.value === null || control.value === undefined || control.value === '') {
        return null;
      }
      const value = Number(control.value);
      return value < max ? null : { lessThan: { max, actual: value } };
    };
  }

  /**
   * Validates that the control value is less than or equal to the specified maximum.
   */
  static lessThanOrEqual(max: number): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (control.value === null || control.value === undefined || control.value === '') {
        return null;
      }
      const value = Number(control.value);
      return value <= max ? null : { lessThanOrEqual: { max, actual: value } };
    };
  }

  /**
   * Validates that the control value is within the specified range (inclusive).
   */
  static rangeValidator(min: number, max: number): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (control.value === null || control.value === undefined || control.value === '') {
        return null;
      }
      const value = Number(control.value);
      return value >= min && value <= max ? null : { range: { min, max, actual: value } };
    };
  }

  /**
   * Validates that the control value matches a specific pattern.
   */
  static patternValidator(pattern: RegExp, errorKey: string): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) {
        return null;
      }
      return pattern.test(control.value) ? null : { [errorKey]: true };
    };
  }

  /**
   * Validates that the control value contains no whitespace.
   */
  static noWhitespace(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) {
        return null;
      }
      const hasWhitespace = /\s/.test(control.value);
      return hasWhitespace ? { noWhitespace: true } : null;
    };
  }

  /**
   * Validates that the control value is not only whitespace.
   */
  static notOnlyWhitespace(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) {
        return null;
      }
      const isOnlyWhitespace = control.value.trim().length === 0;
      return isOnlyWhitespace ? { notOnlyWhitespace: true } : null;
    };
  }
}
