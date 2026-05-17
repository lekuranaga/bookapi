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
  templateUrl: './form.component.html',
  styleUrls: ['./form.component.scss'],
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

    const request$ =
      this.isEdit && this.bookId
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
