import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export class StringValidators {
  /**
   * Validates that the string length is within the specified range.
   */
  static limitedLength(min: number, max: number): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) {
        return null;
      }
      const length = control.value.trim().length;
      if (length < min) {
        return { minLength: { requiredLength: min, actualLength: length } };
      }
      if (length > max) {
        return { maxLength: { requiredLength: max, actualLength: length } };
      }
      return null;
    };
  }

  /**
   * Validates that the string contains only alphanumeric characters.
   */
  static alphanumeric(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) {
        return null;
      }
      const valid = /^[a-zA-Z0-9]+$/.test(control.value);
      return valid ? null : { alphanumeric: true };
    };
  }

  /**
   * Validates that the string contains only alphabetic characters.
   */
  static alphabetic(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) {
        return null;
      }
      const valid = /^[a-zA-Z]+$/.test(control.value);
      return valid ? null : { alphabetic: true };
    };
  }

  /**
   * Validates that the string contains only numeric characters.
   */
  static numeric(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) {
        return null;
      }
      const valid = /^[0-9]+$/.test(control.value);
      return valid ? null : { numeric: true };
    };
  }

  /**
   * Validates that the string starts with a specific prefix.
   */
  static startsWith(prefix: string): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) {
        return null;
      }
      const valid = control.value.startsWith(prefix);
      return valid ? null : { startsWith: { prefix } };
    };
  }

  /**
   * Validates that the string ends with a specific suffix.
   */
  static endsWith(suffix: string): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) {
        return null;
      }
      const valid = control.value.endsWith(suffix);
      return valid ? null : { endsWith: { suffix } };
    };
  }

  /**
   * Validates that the string contains a specific substring.
   */
  static contains(substring: string): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) {
        return null;
      }
      const valid = control.value.includes(substring);
      return valid ? null : { contains: { substring } };
    };
  }

  /**
   * Validates that the string does not contain a specific substring.
   */
  static notContains(substring: string): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) {
        return null;
      }
      const valid = !control.value.includes(substring);
      return valid ? null : { notContains: { substring } };
    };
  }
}
