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
  templateUrl: './list.component.html',
  styleUrls: ['./list.component.scss'],
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
        this.books.update((list) => list.filter((b) => b.id !== book.id));
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
    return new Date(dateStr).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  }
}
