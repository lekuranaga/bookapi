import { Component, inject, signal, OnInit } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { RouterLink, Router, ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';
import { BooksService, BookPayload } from '../../core/api/books.service';
import { ToastService } from '../../shared/toast.service';
import { extractErrorMessage, extractFieldErrors } from '../../core/api/problem-details';

@Component({
  selector: 'app-book-form',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, CommonModule],
  template: `
    <div class="min-h-screen bg-gray-50">
      <nav class="bg-white border-b border-gray-200 px-6 py-4 flex items-center gap-3">
        <a routerLink="/books" class="text-gray-400 hover:text-gray-600 transition-colors text-sm">
          &larr; Books
        </a>
        <span class="text-gray-300">/</span>
        <span class="text-sm font-medium text-gray-900">{{ isEdit ? 'Edit Book' : 'New Book' }}</span>
      </nav>

      <div class="max-w-lg mx-auto px-6 py-8">
        <div class="card p-8">
          <h2 class="text-lg font-semibold text-gray-900 mb-6">{{ isEdit ? 'Edit Book' : 'Add a Book' }}</h2>

          @if (initialLoading()) {
            <p class="text-sm text-gray-400">Loading...</p>
          } @else {
            <form [formGroup]="form" (ngSubmit)="submit()" class="space-y-5">
              <!-- Title -->
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">Title <span class="text-red-500">*</span></label>
                <input
                  type="text"
                  formControlName="title"
                  class="form-input"
                  [class.form-input-error]="hasError('title')"
                  placeholder="Book title"
                />
                @if (submitted && form.controls.title.hasError('required')) {
                  <p class="mt-1 text-xs text-red-600">Title is required.</p>
                }
                @for (msg of serverErrors()['title'] ?? []; track msg) {
                  <p class="mt-1 text-xs text-red-600">{{ msg }}</p>
                }
              </div>

              <!-- Author -->
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">Author <span class="text-red-500">*</span></label>
                <input
                  type="text"
                  formControlName="author"
                  class="form-input"
                  [class.form-input-error]="hasError('author')"
                  placeholder="Author name"
                />
                @if (submitted && form.controls.author.hasError('required')) {
                  <p class="mt-1 text-xs text-red-600">Author is required.</p>
                }
                @for (msg of serverErrors()['author'] ?? []; track msg) {
                  <p class="mt-1 text-xs text-red-600">{{ msg }}</p>
                }
              </div>

              <!-- Rating -->
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">Rating <span class="text-red-500">*</span></label>
                <div class="flex gap-2">
                  @for (n of [1,2,3,4,5]; track n) {
                    <button
                      type="button"
                      (click)="setRating(n)"
                      class="text-2xl leading-none transition-transform hover:scale-110 focus:outline-none"
                      [class.text-yellow-400]="n <= form.controls.rating.value"
                      [class.text-gray-300]="n > form.controls.rating.value"
                    >&#9733;</button>
                  }
                </div>
                @if (submitted && form.controls.rating.hasError('required')) {
                  <p class="mt-1 text-xs text-red-600">Rating is required.</p>
                }
                @if (submitted && (form.controls.rating.hasError('min') || form.controls.rating.hasError('max'))) {
                  <p class="mt-1 text-xs text-red-600">Rating must be between 1 and 5.</p>
                }
                @for (msg of serverErrors()['rating'] ?? []; track msg) {
                  <p class="mt-1 text-xs text-red-600">{{ msg }}</p>
                }
              </div>

              <!-- Read At -->
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">Date Read</label>
                <input
                  type="date"
                  formControlName="readAt"
                  class="form-input"
                  [class.form-input-error]="hasError('readAt')"
                />
                @for (msg of serverErrors()['readAt'] ?? []; track msg) {
                  <p class="mt-1 text-xs text-red-600">{{ msg }}</p>
                }
              </div>

              <!-- Review -->
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">Review</label>
                <textarea
                  formControlName="review"
                  class="form-input min-h-[100px] resize-y"
                  [class.form-input-error]="hasError('review')"
                  placeholder="Your thoughts..."
                ></textarea>
                @for (msg of serverErrors()['review'] ?? []; track msg) {
                  <p class="mt-1 text-xs text-red-600">{{ msg }}</p>
                }
              </div>

              <div class="flex gap-3 pt-2">
                <button type="submit" class="btn-primary" [disabled]="loading()">
                  @if (loading()) { Saving... } @else { {{ isEdit ? 'Save changes' : 'Add book' }} }
                </button>
                <a routerLink="/books" class="btn-secondary">Cancel</a>
              </div>
            </form>
          }
        </div>
      </div>
    </div>
  `,
})
export class BookFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly booksService = inject(BooksService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly toast = inject(ToastService);

  submitted = false;
  loading = signal(false);
  initialLoading = signal(false);
  serverErrors = signal<Record<string, string[] | undefined>>({});
  isEdit = false;
  private bookId: string | null = null;

  form = this.fb.nonNullable.group({
    title: ['', Validators.required],
    author: ['', Validators.required],
    rating: [0, [Validators.required, Validators.min(1), Validators.max(5)]],
    review: [''],
    readAt: [''],
  });

  ngOnInit() {
    this.bookId = this.route.snapshot.paramMap.get('id');
    this.isEdit = !!this.bookId;

    if (this.isEdit && this.bookId) {
      this.initialLoading.set(true);
      this.booksService.getOne(this.bookId).subscribe({
        next: (book) => {
          this.form.patchValue({
            title: book.title,
            author: book.author,
            rating: book.rating,
            review: book.review ?? '',
            readAt: book.readAt ?? '',
          });
          this.initialLoading.set(false);
        },
        error: (err) => {
          this.initialLoading.set(false);
          this.toast.show(extractErrorMessage(err));
          this.router.navigate(['/books']);
        },
      });
    }
  }

  setRating(n: number) {
    this.form.controls.rating.setValue(n);
  }

  hasError(field: string): boolean {
    const ctrl = this.form.get(field);
    return (this.submitted && !!ctrl?.invalid) || (this.serverErrors()[field]?.length ?? 0) > 0;
  }

  submit() {
    this.submitted = true;
    this.serverErrors.set({});
    if (this.form.invalid) return;

    this.loading.set(true);
    const raw = this.form.getRawValue();
    const payload: BookPayload = {
      title: raw.title,
      author: raw.author,
      rating: raw.rating,
      review: raw.review,
      readAt: raw.readAt,
    };

    const request$ = this.isEdit && this.bookId
      ? this.booksService.update(this.bookId, payload)
      : this.booksService.create(payload);

    request$.subscribe({
      next: () => {
        this.toast.show(this.isEdit ? 'Book updated.' : 'Book added.', 'success');
        this.router.navigate(['/books']);
      },
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
