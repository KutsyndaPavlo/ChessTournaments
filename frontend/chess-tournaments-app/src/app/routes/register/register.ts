import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { RegistrationService, RegisterResponse } from '@app/auth/registration.service';

@Component({
  selector: 'app-register',
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register {
  private fb = inject(FormBuilder);
  private registrationService = inject(RegistrationService);
  private router = inject(Router);

  registerForm: FormGroup;
  isLoading = false;
  registrationSuccessful = false;
  errorMessages: string[] = [];

  constructor() {
    this.registerForm = this.fb.group(
      {
        username: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(50)]],
        email: ['', [Validators.required, Validators.email]],
        firstName: ['', [Validators.maxLength(50)]],
        lastName: ['', [Validators.maxLength(50)]],
        password: ['', [Validators.required, Validators.minLength(8)]],
        confirmPassword: ['', [Validators.required]],
      },
      { validators: this.passwordMatchValidator },
    );
  }

  private passwordMatchValidator(group: FormGroup): { [key: string]: boolean } | null {
    const password = group.get('password')?.value;
    const confirmPassword = group.get('confirmPassword')?.value;

    return password === confirmPassword ? null : { passwordMismatch: true };
  }

  onSubmit(): void {
    this.errorMessages = [];

    if (this.registerForm.invalid) {
      this.validateAllFormFields(this.registerForm);
      return;
    }

    this.isLoading = true;
    const formValue = this.registerForm.value;

    this.registrationService.register(formValue).subscribe({
      next: (response: RegisterResponse) => {
        this.isLoading = false;

        if (response.status === 'Succeeded') {
          this.registrationSuccessful = true;
          this.registerForm.reset();
          // Redirect to login after 3 seconds
          setTimeout(() => {
            this.router.navigate(['/']);
          }, 3000);
        } else {
          this.handleErrorResponse(response);
        }
      },
      error: (response: RegisterResponse) => {
        this.isLoading = false;
        this.handleErrorResponse(response);
      },
    });
  }

  private handleErrorResponse(response: RegisterResponse): void {
    if (response.errors && response.errors.length > 0) {
      this.errorMessages = response.errors;
    } else if (response.message) {
      this.errorMessages = [response.message];
    } else {
      this.errorMessages = ['An unexpected error occurred during registration'];
    }
  }

  private validateAllFormFields(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach((field) => {
      const control = formGroup.get(field);
      control?.markAsTouched({ onlySelf: true });
    });
  }

  hasError(fieldName: string, errorType: string): boolean {
    const field = this.registerForm.get(fieldName);
    return !!(field && field.hasError(errorType) && (field.dirty || field.touched));
  }

  hasFormError(errorType: string): boolean {
    return !!(
      this.registerForm.hasError(errorType) &&
      (this.registerForm.get('confirmPassword')?.dirty ||
        this.registerForm.get('confirmPassword')?.touched)
    );
  }
}
