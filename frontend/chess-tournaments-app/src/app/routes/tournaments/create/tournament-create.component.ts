import { Component, ChangeDetectionStrategy, signal, inject, DestroyRef } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  FormControl,
  Validators,
  ReactiveFormsModule,
} from '@angular/forms';
import { Router } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize, tap, catchError, EMPTY } from 'rxjs';
import { Observable } from 'rxjs';

// PrimeNG Imports
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { MessageModule } from 'primeng/message';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { MessageService, ConfirmationService } from 'primeng/api';

// Child Components
import {
  BasicInfoSectionComponent,
  TournamentSettingsSectionComponent,
  PlayerSettingsSectionComponent,
} from './_components';

// Infrastructure & Shared Imports
import { TournamentsService } from '@app/infrastructure/api/services/tournaments.service';
import { TournamentFormat, TimeControl } from '@app/infrastructure/api/models';
import { AuthService } from '@app/auth/auth.service';
import { catchHttp400 } from '@app/infrastructure/errors';
import {
  SUCCESS_TOAST,
  ERROR_TOAST,
  CONFIRM_UNSAVED_CHANGES_MESSAGE,
} from '@app/shared/common/constants';
import { CustomValidators } from '@app/shared/common/validators';
import { CanDeactivateComponent } from '@app/infrastructure/routing/can-deactivate.guard';

interface TournamentFormGroup {
  name: FormControl<string | null>;
  description: FormControl<string | null>;
  startDate: FormControl<Date | null>;
  location: FormControl<string | null>;
  format: FormControl<TournamentFormat | null>;
  timeControl: FormControl<TimeControl | null>;
  timeInMinutes: FormControl<number | null>;
  incrementInSeconds: FormControl<number | null>;
  numberOfRounds: FormControl<number | null>;
  maxPlayers: FormControl<number | null>;
  minPlayers: FormControl<number | null>;
  allowByes: FormControl<boolean | null>;
  entryFee: FormControl<number | null>;
}

@Component({
  selector: 'app-tournament-create',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    ReactiveFormsModule,
    ButtonModule,
    CardModule,
    MessageModule,
    ToastModule,
    ConfirmDialogModule,
    BasicInfoSectionComponent,
    TournamentSettingsSectionComponent,
    PlayerSettingsSectionComponent,
  ],
  providers: [MessageService, ConfirmationService],
  templateUrl: './tournament-create.component.html',
  styleUrls: ['./tournament-create.component.css'],
})
export class TournamentCreateComponent implements CanDeactivateComponent {
  private readonly fb = inject(FormBuilder);
  private readonly tournamentsService = inject(TournamentsService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly messageService = inject(MessageService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly authService = inject(AuthService);

  // Form state signals
  protected loading = signal(false);
  protected error = signal<string | null>(null);
  protected form = signal<FormGroup<TournamentFormGroup> | null>(null);

  // Form options
  protected formatOptions = signal([
    { label: 'Swiss', value: TournamentFormat.Swiss },
    { label: 'Round Robin', value: TournamentFormat.RoundRobin },
    { label: 'Knockout', value: TournamentFormat.Knockout },
  ]);

  protected timeControlOptions = signal([
    { label: 'Bullet', value: TimeControl.Bullet },
    { label: 'Blitz', value: TimeControl.Blitz },
    { label: 'Rapid', value: TimeControl.Rapid },
    { label: 'Classical', value: TimeControl.Classical },
  ]);

  constructor() {
    this.form.set(this.buildForm());

    // Clear error when form changes
    this.form()
      ?.valueChanges.pipe(takeUntilDestroyed())
      .subscribe(() => {
        if (this.error()) {
          this.error.set(null);
        }
      });
  }

  /**
   * Implements CanDeactivateComponent to prevent navigation with unsaved changes.
   */
  canDeactivate(): Observable<boolean> | boolean {
    if (this.form()?.dirty && !this.loading()) {
      return new Observable<boolean>((observer) => {
        this.confirmationService.confirm({
          ...CONFIRM_UNSAVED_CHANGES_MESSAGE,
          accept: () => {
            observer.next(true);
            observer.complete();
          },
          reject: () => {
            observer.next(false);
            observer.complete();
          },
        });
      });
    }
    return true;
  }

  private buildForm(): FormGroup<TournamentFormGroup> {
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);

    return this.fb.group<TournamentFormGroup>({
      name: this.fb.control(null, [
        Validators.required,
        Validators.maxLength(200),
        CustomValidators.notOnlyWhitespace(),
      ]),
      description: this.fb.control(null, [Validators.maxLength(2000)]),
      startDate: this.fb.control(tomorrow, [Validators.required]),
      location: this.fb.control(null, [Validators.maxLength(200)]),
      format: this.fb.control(TournamentFormat.Swiss, [Validators.required]),
      timeControl: this.fb.control(TimeControl.Blitz, [Validators.required]),
      timeInMinutes: this.fb.control(5, [
        Validators.required,
        CustomValidators.rangeValidator(1, 240),
      ]),
      incrementInSeconds: this.fb.control(3, [
        Validators.required,
        CustomValidators.rangeValidator(0, 60),
      ]),
      numberOfRounds: this.fb.control(7, [
        Validators.required,
        CustomValidators.rangeValidator(1, 20),
      ]),
      maxPlayers: this.fb.control(16, [
        Validators.required,
        CustomValidators.rangeValidator(2, 200),
      ]),
      minPlayers: this.fb.control(4, [
        Validators.required,
        CustomValidators.rangeValidator(2, 200),
      ]),
      allowByes: this.fb.control(true),
      entryFee: this.fb.control(0, [Validators.required, CustomValidators.greaterThanOrEqual(0)]),
    });
  }

  protected onSubmit(): void {
    const currentForm = this.form();
    if (!currentForm || currentForm.invalid) {
      currentForm?.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    const formValue = currentForm.getRawValue();
    const organizerId = this.authService.getUserId();

    if (!organizerId) {
      this.error.set('User must be authenticated to create a tournament');
      this.loading.set(false);
      this.messageService.add({
        ...ERROR_TOAST,
        detail: 'User must be authenticated to create a tournament',
      });
      return;
    }

    this.tournamentsService
      .apiTournamentsPost({
        body: {
          name: formValue.name!,
          description: formValue.description || '',
          startDate: formValue.startDate!.toISOString(),
          location: formValue.location || '',
          format: formValue.format!,
          timeControl: formValue.timeControl!,
          timeInMinutes: formValue.timeInMinutes!,
          incrementInSeconds: formValue.incrementInSeconds!,
          numberOfRounds: formValue.numberOfRounds!,
          maxPlayers: formValue.maxPlayers!,
          minPlayers: formValue.minPlayers!,
          allowByes: formValue.allowByes!,
          entryFee: formValue.entryFee!,
          organizerId: organizerId,
        },
      })
      .pipe(
        tap(() => {
          this.messageService.add({
            ...SUCCESS_TOAST,
            detail: `Tournament "${formValue.name}" has been created successfully.`,
          });
          // Mark form as pristine to avoid canDeactivate prompt
          currentForm.markAsPristine();
          this.router.navigate(['/tournaments']);
        }),
        catchHttp400((error) => {
          const errorMessage =
            error.error?.message || 'Validation error occurred. Please check your input.';
          this.error.set(errorMessage);
          this.messageService.add({
            ...ERROR_TOAST,
            detail: errorMessage,
          });
        }),
        catchError((error) => {
          const errorMessage =
            error.error?.message || 'Failed to create tournament. Please try again.';
          this.error.set(errorMessage);
          this.messageService.add({
            ...ERROR_TOAST,
            detail: errorMessage,
          });
          return EMPTY;
        }),
        finalize(() => this.loading.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe();
  }

  protected cancel(): void {
    this.router.navigate(['/tournaments']);
  }
}
