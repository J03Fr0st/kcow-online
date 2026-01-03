import { CommonModule } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  type OnInit,
  signal,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MockDataService } from '@core/services/mock-data.service';
import type { SavedView, TableColumn, User } from '@models/table-data.model';
import { LoadingSpinnerComponent } from '@shared/components/loading-spinner/loading-spinner.component';

type SortDirection = 'asc' | 'desc' | null;

@Component({
  selector: 'app-tables',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, FormsModule, LoadingSpinnerComponent],
  templateUrl: './tables.component.html',
  styleUrls: ['./tables.component.css'],
})
export class TablesComponent implements OnInit {
  private mockDataService = inject(MockDataService);

  // State
  loading = signal(true);
  allUsers = signal<User[]>([]);
  serverSideTotalItems = signal(0);
  currentPage = signal(1);
  itemsPerPage = signal(10);
  searchQuery = signal('');
  statusFilter = signal('');
  sortColumn = signal<string | null>(null);
  sortDirection = signal<SortDirection>(null);
  serverSideMode = signal(false);
  selectedUsers = signal<Set<number>>(new Set());
  currentView = signal<SavedView | null>(null);

  // Table columns with visibility control
  columns: TableColumn[] = [
    { key: 'id', label: 'ID', sortable: true, visible: true },
    { key: 'name', label: 'Name', sortable: true, visible: true },
    { key: 'email', label: 'Email', sortable: true, visible: true },
    { key: 'role', label: 'Role', sortable: true, visible: true },
    { key: 'status', label: 'Status', sortable: true, visible: true },
    { key: 'joinDate', label: 'Join Date', sortable: true, visible: true },
  ];

  // Saved views for quick access
  savedViews: SavedView[] = [
    {
      id: 'default',
      name: 'All Users',
      description: 'Show all users with default sorting',
      filters: {},
      isDefault: true,
    },
    {
      id: 'active-users',
      name: 'Active Users',
      description: 'Show only active users',
      filters: { statusFilter: 'active' },
      sort: { column: 'name', direction: 'asc' },
    },
    {
      id: 'pending-review',
      name: 'Pending Review',
      description: 'Users awaiting approval',
      filters: { statusFilter: 'pending' },
      sort: { column: 'joinDate', direction: 'desc' },
    },
    {
      id: 'admins-managers',
      name: 'Admin & Managers',
      description: 'High-privilege users',
      filters: {},
      columns: ['name', 'email', 'role', 'status'],
    },
    {
      id: 'recent-joins',
      name: 'Recent Joins',
      description: 'Users who joined recently',
      filters: {},
      sort: { column: 'joinDate', direction: 'desc' },
    },
  ];

  // Computed values for visible columns
  visibleColumns = computed(() => this.columns.filter((c) => c.visible));
  visibleColumnsCount = computed(() => this.visibleColumns().length);

  // Client-side filtering and sorting
  filteredUsers = computed(() => {
    if (this.serverSideMode()) {
      return this.allUsers();
    }

    let users = this.allUsers();

    // Apply search filter
    const search = this.searchQuery().toLowerCase();
    if (search) {
      users = users.filter(
        (u) => u.name.toLowerCase().includes(search) || u.email.toLowerCase().includes(search),
      );
    }

    // Apply status filter
    const status = this.statusFilter();
    if (status) {
      users = users.filter((u) => u.status === status);
    }

    // Apply sorting
    const column = this.sortColumn();
    const direction = this.sortDirection();
    if (column && direction) {
      users = [...users].sort((a, b) => {
        const aVal = a[column as keyof User];
        const bVal = b[column as keyof User];
        const comparison = aVal > bVal ? 1 : aVal < bVal ? -1 : 0;
        return direction === 'asc' ? comparison : -comparison;
      });
    }

    return users;
  });

  totalItems = computed(() =>
    this.serverSideMode() ? this.serverSideTotalItems() : this.filteredUsers().length,
  );
  totalPages = computed(() => Math.ceil(this.totalItems() / this.itemsPerPage()));
  startIndex = computed(() => (this.currentPage() - 1) * this.itemsPerPage());
  endIndex = computed(() => Math.min(this.startIndex() + this.itemsPerPage(), this.totalItems()));

  displayedUsers = computed(() => {
    if (this.serverSideMode()) {
      return this.allUsers();
    }
    const start = this.startIndex();
    const end = this.endIndex();
    return this.filteredUsers().slice(start, end);
  });

  visiblePages = computed(() => {
    const current = this.currentPage();
    const total = this.totalPages();
    const pages: number[] = [];

    let start = Math.max(1, current - 2);
    let end = Math.min(total, current + 2);

    if (current <= 3) {
      end = Math.min(5, total);
    }
    if (current >= total - 2) {
      start = Math.max(total - 4, 1);
    }

    for (let i = start; i <= end; i++) {
      pages.push(i);
    }

    return pages;
  });

  // Selection helpers
  isAllSelected = computed(() => {
    const displayed = this.displayedUsers();
    return displayed.length > 0 && displayed.every((u) => this.selectedUsers().has(u.id));
  });

  isSomeSelected = computed(() => {
    const displayed = this.displayedUsers();
    const selectedCount = displayed.filter((u) => this.selectedUsers().has(u.id)).length;
    return selectedCount > 0 && selectedCount < displayed.length;
  });

  // Effect to reload data when filters change in server-side mode
  constructor() {
    effect(() => {
      if (this.serverSideMode()) {
        this.loadServerSideData();
      }
    });
  }

  ngOnInit(): void {
    this.loadUsers();
  }

  private loadUsers(): void {
    if (this.serverSideMode()) {
      this.loadServerSideData();
    } else {
      this.mockDataService.getUsers(1, 50).subscribe({
        next: (users) => {
          this.allUsers.set(users);
          this.loading.set(false);
        },
        error: () => {
          this.loading.set(false);
        },
      });
    }
  }

  private loadServerSideData(): void {
    this.loading.set(true);
    this.mockDataService
      .getUsersWithPagination(
        this.currentPage(),
        this.itemsPerPage(),
        this.searchQuery(),
        this.statusFilter(),
        this.sortColumn(),
        this.sortDirection() as 'asc' | 'desc' | null,
      )
      .subscribe({
        next: (response) => {
          this.allUsers.set(response.data);
          this.serverSideTotalItems.set(response.total);
          this.loading.set(false);
        },
        error: () => {
          this.loading.set(false);
        },
      });
  }

  toggleServerSideMode(): void {
    this.serverSideMode.update((v) => !v);
    this.currentPage.set(1);
    this.clearSelection();
    this.loadUsers();
  }

  onSort(column: string): void {
    if (this.sortColumn() === column) {
      const current = this.sortDirection();
      this.sortDirection.set(current === 'asc' ? 'desc' : current === 'desc' ? null : 'asc');
      if (this.sortDirection() === null) {
        this.sortColumn.set(null);
      }
    } else {
      this.sortColumn.set(column);
      this.sortDirection.set('asc');
    }
    this.currentPage.set(1);
  }

  onSearchChange(): void {
    this.currentPage.set(1);
  }

  onFilterChange(): void {
    this.currentPage.set(1);
  }

  onItemsPerPageChange(): void {
    this.currentPage.set(1);
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages()) {
      this.currentPage.set(page);
      if (this.serverSideMode()) {
        this.loadServerSideData();
      }
    }
  }

  // Column visibility management
  toggleColumnVisibility(key: string): void {
    const column = this.columns.find((c) => c.key === key);
    if (column) {
      column.visible = !column.visible;
    }
  }

  resetColumns(): void {
    this.columns.forEach((c) => {
      c.visible = true;
    });
  }

  isColumnVisible(key: string): boolean {
    return this.columns.find((c) => c.key === key)?.visible ?? false;
  }

  // Selection management
  toggleUserSelection(userId: number): void {
    this.selectedUsers.update((selected) => {
      const newSet = new Set(selected);
      if (newSet.has(userId)) {
        newSet.delete(userId);
      } else {
        newSet.add(userId);
      }
      return newSet;
    });
  }

  toggleAllSelection(): void {
    const displayed = this.displayedUsers();
    if (this.isAllSelected()) {
      this.selectedUsers.update((selected) => {
        const newSet = new Set(selected);
        displayed.forEach((u) => {
          newSet.delete(u.id);
        });
        return newSet;
      });
    } else {
      this.selectedUsers.update((selected) => {
        const newSet = new Set(selected);
        displayed.forEach((u) => {
          newSet.add(u.id);
        });
        return newSet;
      });
    }
  }

  clearSelection(): void {
    this.selectedUsers.set(new Set());
  }

  // Bulk actions
  bulkDelete(): void {
    if (confirm(`Delete ${this.selectedUsers().size} selected users?`)) {
      this.allUsers.update((users) => users.filter((u) => !this.selectedUsers().has(u.id)));
      this.clearSelection();
    }
  }

  bulkExport(): void {
    const selectedIds = Array.from(this.selectedUsers());
    const users = this.allUsers().filter((u) => selectedIds.includes(u.id));
    const csv = this.convertToCSV(users);
    this.downloadCSV(csv, 'selected-users.csv');
  }

  bulkUpdateStatus(status: 'active' | 'inactive' | 'pending'): void {
    this.allUsers.update((users) =>
      users.map((u) => (this.selectedUsers().has(u.id) ? { ...u, status } : u)),
    );
    this.clearSelection();
  }

  deleteUser(userId: number): void {
    if (confirm('Delete this user?')) {
      this.allUsers.update((users) => users.filter((u) => u.id !== userId));
    }
  }

  // Export functionality
  exportData(): void {
    const users = this.serverSideMode() ? this.allUsers() : this.filteredUsers();
    const csv = this.convertToCSV(users);
    this.downloadCSV(csv, 'users.csv');
  }

  private convertToCSV(users: User[]): string {
    const visibleCols = this.visibleColumns();
    const headers = visibleCols.map((c) => c.label);
    const rows = users.map((u) => visibleCols.map((c) => u[c.key as keyof User]));
    return [headers, ...rows].map((row) => row.join(',')).join('\n');
  }

  private downloadCSV(csv: string, filename: string): void {
    const blob = new Blob([csv], { type: 'text/csv' });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    link.click();
    window.URL.revokeObjectURL(url);
  }

  // Saved views management
  applyView(view: SavedView): void {
    this.currentView.set(view);

    // Apply filters
    this.searchQuery.set(view.filters.searchQuery || '');
    this.statusFilter.set(view.filters.statusFilter || '');

    // Apply sorting
    if (view.sort) {
      this.sortColumn.set(view.sort.column);
      this.sortDirection.set(view.sort.direction);
    } else {
      this.sortColumn.set(null);
      this.sortDirection.set(null);
    }

    // Apply column visibility
    if (view.columns) {
      this.columns.forEach((c) => {
        c.visible = view.columns?.includes(c.key);
      });
    } else {
      this.resetColumns();
    }

    this.currentPage.set(1);
  }

  saveCurrentView(): void {
    const name = prompt('Enter a name for this view:');
    if (name) {
      const newView: SavedView = {
        id: `custom-${Date.now()}`,
        name,
        filters: {
          searchQuery: this.searchQuery() || undefined,
          statusFilter: this.statusFilter() || undefined,
        },
        sort: (() => {
          const column = this.sortColumn();
          return column
            ? {
                column,
                direction: this.sortDirection() as 'asc' | 'desc',
              }
            : undefined;
        })(),
        columns: this.columns.filter((c) => c.visible).map((c) => c.key),
      };
      this.savedViews.push(newView);
      this.currentView.set(newView);
      alert(`View "${name}" saved successfully!`);
    }
  }
}
