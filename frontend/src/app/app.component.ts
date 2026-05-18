import { Component, inject } from '@angular/core';
import { RouterOutlet, Router, NavigationEnd } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ToastContainerComponent } from './shared/toast-container.component';
import { AuthService } from './core/auth/auth.service';
import { toSignal } from '@angular/core/rxjs-interop';
import { map, filter } from 'rxjs/operators';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, ToastContainerComponent, CommonModule],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
})
export class AppComponent {
  readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly showNav = toSignal(
    this.router.events.pipe(
      filter((e) => e instanceof NavigationEnd),
      map((e) => {
        const url = (e as NavigationEnd).urlAfterRedirects;
        return !url.startsWith('/login') && !url.startsWith('/register');
      }),
    ),
    { initialValue: false },
  );
}
