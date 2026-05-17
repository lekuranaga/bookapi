import { Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { RouterLink, Router } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { ToastService } from '../../shared/toast.service';
import { extractErrorMessage, extractFieldErrors } from '../../core/api/problem-details';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss'],
})
export class RegisterComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  submitted = false;
  loading = signal(false);
  serverErrors = signal<Record<string, string[] | undefined>>({});

  form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required],
  });

  hasError(field: string): boolean {
    const ctrl = this.form.get(field);
    return (this.submitted && !!ctrl?.invalid) || (this.serverErrors()[field]?.length ?? 0) > 0;
  }

  submit() {
    this.submitted = true;
    this.serverErrors.set({});
    if (this.form.invalid) return;

    this.loading.set(true);
    const { email, password } = this.form.getRawValue();

    this.authService.register(email, password).subscribe({
      next: () => this.router.navigate(['/books']),
      error: (err) => {
        this.loading.set(false);
        const fieldErrs = extractFieldErrors(err);
        if (Object.keys(fieldErrs).length) {
          this.serverErrors.set(fieldErrs as Record<string, string[] | undefined>);
        } else {
          this.toast.show(extractErrorMessage(err));
        }
      },
    });
  }
}
