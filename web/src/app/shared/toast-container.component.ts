import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastService } from './toast.service';

@Component({
  selector: 'app-toast-container',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="fixed top-4 right-4 z-50 flex flex-col gap-2 w-80">
      @for (toast of toastService.toasts(); track toast.id) {
        <div
          class="flex items-start gap-3 rounded-lg px-4 py-3 shadow-md text-sm font-medium"
          [class]="toastClass(toast.type)"
        >
          <span class="flex-1">{{ toast.message }}</span>
          <button
            (click)="toastService.dismiss(toast.id)"
            class="shrink-0 opacity-70 hover:opacity-100 transition-opacity"
          >
            &times;
          </button>
        </div>
      }
    </div>
  `,
})
export class ToastContainerComponent {
  readonly toastService = inject(ToastService);

  toastClass(type: string): string {
    switch (type) {
      case 'error': return 'bg-red-600 text-white';
      case 'success': return 'bg-green-600 text-white';
      default: return 'bg-gray-800 text-white';
    }
  }
}
