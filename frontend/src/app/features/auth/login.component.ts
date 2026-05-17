import { Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { RouterLink, Router } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { ToastService } from '../../shared/toast.service';
import { extractErrorMessage } from '../../core/api/problem-details';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-gray-50 px-4">
      <div class="card w-full max-w-md p-8">
        <div class="mb-8 text-center">
          <h1 class="text-2xl font-bold text-gray-900">BookTracker</h1>
          <p class="mt-1 text-sm text-gray-500">Sign in to your account</p>
        </div>

        <form [formGroup]="form" (ngSubmit)="submit()" class="space-y-5">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Email</label>
            <input
              type="email"
              formControlName="email"
              class="form-input"
              [class.form-input-error]="submitted && form.controls.email.invalid"
              placeholder="you@example.com"
              autocomplete="email"
            />
            @if (submitted && form.controls.email.hasError('required')) {
              <p class="mt-1 text-xs text-red-600">Email is required.</p>
            }
            @if (submitted && form.controls.email.hasError('email')) {
              <p class="mt-1 text-xs text-red-600">Enter a valid email.</p>
            }
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Password</label>
            <input
              type="password"
              formControlName="password"
              class="form-input"
              [class.form-input-error]="submitted && form.controls.password.invalid"
              placeholder="••••••••"
              autocomplete="current-password"
            />
            @if (submitted && form.controls.password.hasError('required')) {
              <p class="mt-1 text-xs text-red-600">Password is required.</p>
            }
          </div>

          <button type="submit" class="btn-primary w-full" [disabled]="loading()">
            @if (loading()) { Signing in... } @else { Sign in }
          </button>
        </form>

        <p class="mt-6 text-center text-sm text-gray-500">
          No account? <a routerLink="/register" class="text-indigo-600 hover:underline font-medium">Register</a>
        </p>
      </div>
    </div>
  `,
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  submitted = false;
  loading = signal(false);

  form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required],
  });

  submit() {
    this.submitted = true;
    if (this.form.invalid) return;

    this.loading.set(true);
    const { email, password } = this.form.getRawValue();

    this.authService.login(email, password).subscribe({
      next: () => this.router.navigate(['/books']),
      error: (err) => {
        this.loading.set(false);
        this.toast.show(extractErrorMessage(err));
      },
    });
  }
}
