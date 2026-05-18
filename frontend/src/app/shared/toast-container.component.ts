import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastService } from './toast.service';

@Component({
  selector: 'app-toast-container',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './toast-container.component.html',
  styleUrls: ['./toast-container.component.scss'],
})
export class ToastContainerComponent {
  readonly toastService = inject(ToastService);

  toastClass(type: string): string {
    switch (type) {
      case 'error':
        return 'bg-error text-on-error';
      case 'success':
        return 'bg-success text-white';
      default:
        return 'bg-inverse-surface text-inverse-on-surface';
    }
  }
}
