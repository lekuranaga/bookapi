import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

export interface BookDto {
  id: string;
  title: string;
  author: string;
  rating: number;
  review: string;
  readAt: string;
  createdAt: string;
  updatedAt: string;
}

export interface BookPayload {
  title: string;
  author: string;
  rating: number;
  review: string;
  readAt: string;
}

@Injectable({ providedIn: 'root' })
export class BooksService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/api/books`;

  getAll() {
    return this.http.get<BookDto[]>(this.base);
  }

  getOne(id: string) {
    return this.http.get<BookDto>(`${this.base}/${id}`);
  }

  create(payload: BookPayload) {
    return this.http.post<BookDto>(this.base, payload);
  }

  update(id: string, payload: BookPayload) {
    return this.http.put<BookDto>(`${this.base}/${id}`, payload);
  }

  delete(id: string) {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
