import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { BooksService, BookDto } from '../../core/api/books.service';
import { AuthService } from '../../core/auth/auth.service';
import { ToastService } from '../../shared/toast.service';
import { extractErrorMessage } from '../../core/api/problem-details';

@Component({
  selector: 'app-books-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="min-h-screen bg-gray-50">
      <!-- Nav -->
      <nav class="bg-white border-b border-gray-200 px-6 py-4 flex items-center justify-between">
        <span class="text-lg font-bold text-gray-900">BookTracker</span>
        <div class="flex items-center gap-4">
          <span class="text-sm text-gray-500">{{ authService.user()?.email }}</span>
          <button (click)="authService.logout()" class="btn-secondary text-xs px-3 py-1.5">
            Sign out
          </button>
        </div>
      </nav>

      <div class="max-w-5xl mx-auto px-6 py-8">
        <div class="flex items-center justify-between mb-6">
          <h2 class="text-xl font-semibold text-gray-900">My Books</h2>
          <a routerLink="/books/new" class="btn-primary">+ New Book</a>
        </div>

        @if (loading()) {
          <div class="card p-8 text-center text-gray-400 text-sm">Loading books...</div>
        } @else if (books().length === 0) {
          <div class="card p-12 text-center">
            <p class="text-gray-400 text-sm">No books yet. Add your first read!</p>
            <a routerLink="/books/new" class="btn-primary mt-4 inline-block">Add a book</a>
          </div>
        } @else {
          <div class="card overflow-hidden">
            <table class="w-full text-sm">
              <thead class="bg-gray-50 border-b border-gray-200">
                <tr>
                  <th class="px-4 py-3 text-left font-medium text-gray-600">Title</th>
                  <th class="px-4 py-3 text-left font-medium text-gray-600">Author</th>
                  <th class="px-4 py-3 text-left font-medium text-gray-600">Rating</th>
                  <th class="px-4 py-3 text-left font-medium text-gray-600">Read on</th>
                  <th class="px-4 py-3 text-right font-medium text-gray-600">Actions</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-gray-100">
                @for (book of books(); track book.id) {
                  <tr class="hover:bg-gray-50 transition-colors">
                    <td class="px-4 py-3 font-medium text-gray-900">{{ book.title }}</td>
                    <td class="px-4 py-3 text-gray-600">{{ book.author }}</td>
                    <td class="px-4 py-3">
                      <span class="text-yellow-500 tracking-tighter">{{ stars(book.rating) }}</span>
                      <span class="text-gray-300 tracking-tighter">{{ emptyStars(book.rating) }}</span>
                    </td>
                    <td class="px-4 py-3 text-gray-500">{{ formatDate(book.readAt) }}</td>
                    <td class="px-4 py-3 text-right flex justify-end gap-2">
                      <a [routerLink]="['/books', book.id, 'edit']" class="btn-secondary text-xs px-3 py-1.5">
                        Edit
                      </a>
                      <button
                        (click)="confirmDelete(book)"
                        class="btn-danger text-xs px-3 py-1.5"
                        [disabled]="deletingId() === book.id"
                      >
                        @if (deletingId() === book.id) { Deleting... } @else { Delete }
                      </button>
                    </td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        }
      </div>
    </div>
  `,
})
export class BooksListComponent implements OnInit {
  private readonly booksService = inject(BooksService);
  readonly authService = inject(AuthService);
  private readonly toast = inject(ToastService);

  books = signal<BookDto[]>([]);
  loading = signal(true);
  deletingId = signal<string | null>(null);

  ngOnInit() {
    this.loadBooks();
  }

  loadBooks() {
    this.loading.set(true);
    this.booksService.getAll().subscribe({
      next: (data) => {
        this.books.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.toast.show(extractErrorMessage(err));
      },
    });
  }

  confirmDelete(book: BookDto) {
    if (!confirm(`Delete "${book.title}"?`)) return;
    this.deletingId.set(book.id);
    this.booksService.delete(book.id).subscribe({
      next: () => {
        this.books.update(list => list.filter(b => b.id !== book.id));
        this.deletingId.set(null);
        this.toast.show('Book deleted.', 'success');
      },
      error: (err) => {
        this.deletingId.set(null);
        this.toast.show(extractErrorMessage(err));
      },
    });
  }

  stars(rating: number): string {
    return '★'.repeat(rating);
  }

  emptyStars(rating: number): string {
    return '★'.repeat(5 - rating);
  }

  formatDate(dateStr: string): string {
    if (!dateStr) return '-';
    return new Date(dateStr).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' });
  }
}
