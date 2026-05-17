import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'books', pathMatch: 'full' },
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login.component').then(m => m.LoginComponent),
  },
  {
    path: 'register',
    loadComponent: () => import('./features/auth/register.component').then(m => m.RegisterComponent),
  },
  {
    path: 'books',
    canActivate: [authGuard],
    loadComponent: () => import('./features/books/list.component').then(m => m.BooksListComponent),
  },
  {
    path: 'books/new',
    canActivate: [authGuard],
    loadComponent: () => import('./features/books/form.component').then(m => m.BookFormComponent),
  },
  {
    path: 'books/:id/edit',
    canActivate: [authGuard],
    loadComponent: () => import('./features/books/form.component').then(m => m.BookFormComponent),
  },
  { path: '**', redirectTo: 'books' },
];
