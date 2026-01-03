export interface TableColumn {
  key: string;
  label: string;
  sortable?: boolean;
  width?: string;
  visible?: boolean;
}

export interface User {
  id: number;
  name: string;
  email: string;
  role: string;
  status: 'active' | 'inactive' | 'pending';
  joinDate: string;
}

export interface PaginationConfig {
  currentPage: number;
  itemsPerPage: number;
  totalItems: number;
}

export interface SavedView {
  id: string;
  name: string;
  description?: string;
  filters: {
    searchQuery?: string;
    statusFilter?: string;
  };
  sort?: {
    column: string;
    direction: 'asc' | 'desc';
  };
  columns?: string[];
  isDefault?: boolean;
}

export interface ServerPaginationResponse<T> {
  data: T[];
  total: number;
  page: number;
  pageSize: number;
}
