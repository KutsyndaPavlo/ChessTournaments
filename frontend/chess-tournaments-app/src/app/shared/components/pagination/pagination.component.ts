import { Component, EventEmitter, Input, Output, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-pagination',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './pagination.component.html',
  styleUrls: ['./pagination.component.css'],
})
export class PaginationComponent {
  private _totalPages = signal(0);
  private _currentPage = signal(1);
  private _totalCount = signal(0);
  private _pageSize = signal(20);

  @Input() set totalPages(value: number) {
    this._totalPages.set(value);
  }
  get totalPages(): number {
    return this._totalPages();
  }

  @Input() set currentPage(value: number) {
    this._currentPage.set(value);
  }
  get currentPage(): number {
    return this._currentPage();
  }

  @Input() set totalCount(value: number) {
    this._totalCount.set(value);
  }
  get totalCount(): number {
    return this._totalCount();
  }

  @Input() set pageSize(value: number) {
    this._pageSize.set(value);
  }
  get pageSize(): number {
    return this._pageSize();
  }

  @Input() showPageSize = true;
  @Input() pageSizeOptions = [10, 20, 50, 100];

  @Output() pageChange = new EventEmitter<number>();
  @Output() pageSizeChange = new EventEmitter<number>();

  hasPreviousPage = computed(() => this._currentPage() > 1);
  hasNextPage = computed(() => this._currentPage() < this._totalPages());

  // Calculate visible page numbers
  visiblePages = computed(() => {
    const current = this._currentPage();
    const total = this._totalPages();
    const pages: (number | string)[] = [];

    if (total <= 7) {
      // Show all pages if 7 or fewer
      for (let i = 1; i <= total; i++) {
        pages.push(i);
      }
    } else {
      // Always show first page
      pages.push(1);

      if (current > 3) {
        pages.push('...');
      }

      // Show current page and neighbors
      const start = Math.max(2, current - 1);
      const end = Math.min(total - 1, current + 1);

      for (let i = start; i <= end; i++) {
        pages.push(i);
      }

      if (current < total - 2) {
        pages.push('...');
      }

      // Always show last page
      if (total > 1) {
        pages.push(total);
      }
    }

    return pages;
  });

  // Calculate range display
  rangeStart = computed(() => {
    const current = this._currentPage();
    const size = this._pageSize();
    return (current - 1) * size + 1;
  });

  rangeEnd = computed(() => {
    const current = this._currentPage();
    const size = this._pageSize();
    const total = this._totalCount();
    return Math.min(current * size, total);
  });

  goToPage(page: number | string): void {
    if (
      typeof page === 'number' &&
      page !== this._currentPage() &&
      page >= 1 &&
      page <= this._totalPages()
    ) {
      this.pageChange.emit(page);
    }
  }

  goToFirst(): void {
    if (this.hasPreviousPage()) {
      this.pageChange.emit(1);
    }
  }

  goToPrevious(): void {
    if (this.hasPreviousPage()) {
      this.pageChange.emit(this._currentPage() - 1);
    }
  }

  goToNext(): void {
    if (this.hasNextPage()) {
      this.pageChange.emit(this._currentPage() + 1);
    }
  }

  goToLast(): void {
    if (this.hasNextPage()) {
      this.pageChange.emit(this._totalPages());
    }
  }

  onPageSizeChange(event: Event): void {
    const select = event.target as HTMLSelectElement;
    const newSize = parseInt(select.value, 10);
    if (newSize !== this._pageSize()) {
      this.pageSizeChange.emit(newSize);
    }
  }
}
