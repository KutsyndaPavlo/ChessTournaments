import { Component, OnInit, inject, ChangeDetectionStrategy, signal } from '@angular/core';
import { Router } from '@angular/router';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { AuthService } from '@app/auth/auth.service';

@Component({
  selector: 'app-callback',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ProgressSpinnerModule],
  template: `
    <div class="flex min-h-[calc(100vh-120px)] items-center justify-center p-8">
      <div class="flex flex-col items-center text-center">
        <!-- Chess-themed loading spinner -->
        <div class="mb-8">
          @if (!error()) {
            <div class="relative">
              <span class="chess-piece text-6xl">♔</span>
            </div>
          } @else {
            <span class="text-6xl">⚠</span>
          }
        </div>

        @if (!error()) {
          <h2 class="mb-2 text-2xl font-bold text-gray-800">Processing authentication...</h2>
          <p class="text-gray-500">Please wait while we complete your sign in.</p>
        } @else {
          <h2 class="mb-2 text-2xl font-bold text-red-600">Authentication Error</h2>
          <p class="mb-4 text-gray-500">{{ error() }}</p>
          <p class="text-sm text-gray-400">Redirecting to home page...</p>
        }
      </div>
    </div>
  `,
  styles: `
    .chess-piece {
      display: inline-block;
      background: linear-gradient(135deg, #4f46e5, #7c3aed);
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
      background-clip: text;
      animation: pulse 1.5s ease-in-out infinite;
    }

    @keyframes pulse {
      0%,
      100% {
        opacity: 1;
        transform: scale(1);
      }
      50% {
        opacity: 0.6;
        transform: scale(1.1);
      }
    }
  `,
})
export class CallbackComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  protected error = signal<string | null>(null);

  ngOnInit(): void {
    this.handleAuthCallback();
  }

  private handleAuthCallback(): void {
    // Give the auth service time to process the callback
    setTimeout(() => {
      if (this.authService.hasValidToken()) {
        this.router.navigate(['/tournaments']);
      } else {
        this.error.set('Authentication failed. Please try again.');
        // Redirect to home after showing error
        setTimeout(() => {
          this.router.navigate(['/']);
        }, 2000);
      }
    }, 1000);
  }
}
