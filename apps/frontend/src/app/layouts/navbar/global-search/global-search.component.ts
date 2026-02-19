import { CommonModule } from '@angular/common';
import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  computed,
  HostListener,
  inject,
  type OnDestroy,
  type OnInit,
  signal,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { debounceTime, distinctUntilChanged, Subject, takeUntil } from 'rxjs';
import { type StudentSearchResult, StudentService } from '../../../core/services/student.service';

@Component({
  selector: 'app-global-search',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './global-search.component.html',
  styleUrls: ['./global-search.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class GlobalSearchComponent implements OnInit, OnDestroy {
  private readonly studentService = inject(StudentService);
  private readonly router = inject(Router);
  private readonly cdr = inject(ChangeDetectorRef);

  // Signals for state
  searchTerm = signal('');
  results = signal<StudentSearchResult[]>([]);
  isLoading = signal(false);
  isOpen = signal(false);
  highlightedIndex = signal<number>(-1);

  // Subject for debouncing
  private searchSubject = new Subject<string>();
  private destroy$ = new Subject<void>();

  // Computed signals
  readonly hasResults = computed(() => this.results().length > 0);
  readonly showNoResults = computed(
    () => !this.isLoading() && this.searchTerm().trim().length >= 2 && this.results().length === 0,
  );

  ngOnInit(): void {
    this.searchSubject
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntil(this.destroy$))
      .subscribe((term) => {
        this.performSearch(term);
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onSearchChange(term: string): void {
    this.searchTerm.set(term);

    if (term.length >= 2) {
      this.isLoading.set(true);
      this.isOpen.set(true);
      this.searchSubject.next(term);
    } else {
      this.results.set([]);
      this.isOpen.set(false);
      this.highlightedIndex.set(-1);
    }
  }

  private performSearch(term: string): void {
    this.studentService.searchStudents(term, 10).subscribe({
      next: (searchResults) => {
        this.results.set(searchResults);
        this.isLoading.set(false);
        this.highlightedIndex.set(-1);
        this.cdr.markForCheck();
      },
      error: () => {
        this.results.set([]);
        this.isLoading.set(false);
        this.cdr.markForCheck();
      },
    });
  }

  selectResult(result: StudentSearchResult): void {
    this.router.navigate(['/students', result.id]);
    this.clearSearch();
  }

  clearSearch(): void {
    this.searchTerm.set('');
    this.results.set([]);
    this.isOpen.set(false);
    this.highlightedIndex.set(-1);
  }

  onKeyDown(event: KeyboardEvent): void {
    const resultsList = this.results();
    const currentIndex = this.highlightedIndex();

    switch (event.key) {
      case 'ArrowDown':
        event.preventDefault();
        if (currentIndex < resultsList.length - 1) {
          this.highlightedIndex.set(currentIndex + 1);
        }
        break;
      case 'ArrowUp':
        event.preventDefault();
        if (currentIndex > 0) {
          this.highlightedIndex.set(currentIndex - 1);
        }
        break;
      case 'Enter':
        event.preventDefault();
        if (currentIndex >= 0 && currentIndex < resultsList.length) {
          this.selectResult(resultsList[currentIndex]);
        }
        break;
      case 'Escape':
        this.clearSearch();
        break;
    }
  }

  highlightMatch(text: string, query: string): string {
    if (!query.trim()) return text;

    const regex = new RegExp(`(${this.escapeRegex(query)})`, 'gi');
    return text.replace(regex, '<mark>$1</mark>');
  }

  private escapeRegex(text: string): string {
    return text.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
  }

  onFocus(): void {
    if (this.searchTerm().trim().length >= 2) {
      this.isOpen.set(true);
    }
  }

  @HostListener('document:click', ['$event'])
  onClickOutside(event: MouseEvent): void {
    const target = event.target as HTMLElement;
    const searchContainer = document.querySelector('.global-search-container');

    if (searchContainer && !searchContainer.contains(target)) {
      this.isOpen.set(false);
    }
  }
}
