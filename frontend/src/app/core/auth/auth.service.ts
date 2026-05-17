import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { AuthResponse, Session, AuthUser } from './models';

const SESSION_KEY = 'bt_session';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  private readonly _session = signal<Session | null>(this.loadSession());

  readonly user = computed<AuthUser | null>(() => this._session()?.user ?? null);
  readonly token = computed<string | null>(() => this._session()?.token ?? null);
  readonly isAuthenticated = computed<boolean>(() => {
    const s = this._session();
    if (!s) return false;
    return new Date(s.expiresAt) > new Date();
  });

  private loadSession(): Session | null {
    try {
      const raw = localStorage.getItem(SESSION_KEY);
      if (!raw) return null;
      const s: Session = JSON.parse(raw);
      if (new Date(s.expiresAt) <= new Date()) {
        localStorage.removeItem(SESSION_KEY);
        return null;
      }
      return s;
    } catch {
      return null;
    }
  }

  login(email: string, password: string) {
    return this.http
      .post<AuthResponse>(`${environment.apiBaseUrl}/api/v1/auth/login`, { email, password })
      .pipe(tap(res => this.persist(res)));
  }

  register(email: string, password: string) {
    return this.http
      .post<AuthResponse>(`${environment.apiBaseUrl}/api/v1/auth/register`, { email, password })
      .pipe(tap(res => this.persist(res)));
  }

  logout() {
    this._session.set(null);
    localStorage.removeItem(SESSION_KEY);
    this.router.navigate(['/login']);
  }

  clearSession() {
    this._session.set(null);
    localStorage.removeItem(SESSION_KEY);
  }

  private persist(res: AuthResponse) {
    const session: Session = {
      token: res.accessToken,
      expiresAt: res.expiresAt,
      user: { userId: res.userId, email: res.email },
    };
    this._session.set(session);
    localStorage.setItem(SESSION_KEY, JSON.stringify(session));
  }
}
