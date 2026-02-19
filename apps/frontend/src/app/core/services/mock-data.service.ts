import { Injectable } from '@angular/core';
import type { RecentActivity } from '@models/recent-activity.model';
import type { StatCard } from '@models/stats.model';
import type { ServerPaginationResponse, User } from '@models/table-data.model';
import { type Observable, of } from 'rxjs';
import { delay } from 'rxjs/operators';

@Injectable({
  providedIn: 'root',
})
export class MockDataService {
  getRecentActivities(): Observable<RecentActivity[]> {
    const activities: RecentActivity[] = [
      { id: 1, user: 'JD', action: 'Created new project', time: '2 minutes ago' },
      { id: 2, user: 'AB', action: 'Updated user profile', time: '15 minutes ago' },
      { id: 3, user: 'CD', action: 'Uploaded new file', time: '1 hour ago' },
      { id: 4, user: 'EF', action: 'Completed task', time: '2 hours ago' },
    ];
    return of(activities).pipe(delay(300));
  }

  getStats(): Observable<StatCard[]> {
    const stats: StatCard[] = [
      {
        title: 'Total Revenue',
        value: '$45,231',
        change: 12.5,
        changeType: 'increase',
        icon: 'dollar',
        color: 'primary',
      },
      {
        title: 'Active Users',
        value: '2,543',
        change: 8.2,
        changeType: 'increase',
        icon: 'users',
        color: 'success',
      },
      {
        title: 'Total Orders',
        value: '1,234',
        change: 3.1,
        changeType: 'decrease',
        icon: 'shopping',
        color: 'accent',
      },
      {
        title: 'Conversion Rate',
        value: '3.24%',
        change: 1.5,
        changeType: 'increase',
        icon: 'trending',
        color: 'info',
      },
    ];

    return of(stats).pipe(delay(300));
  }

  getUsers(page = 1, limit = 10): Observable<User[]> {
    // Deterministic fixture data for consistent screenshots, exports, and tests
    const users: User[] = this.getUserFixtures();

    const start = (page - 1) * limit;
    const end = start + limit;

    return of(users.slice(start, end)).pipe(delay(300));
  }

  /**
   * Server-side pagination with filtering and sorting
   */
  getUsersWithPagination(
    page = 1,
    limit = 10,
    searchQuery = '',
    statusFilter = '',
    sortColumn: string | null = null,
    sortDirection: 'asc' | 'desc' | null = null,
  ): Observable<ServerPaginationResponse<User>> {
    let users: User[] = this.getUserFixtures();

    // Apply search filter
    if (searchQuery) {
      const search = searchQuery.toLowerCase();
      users = users.filter(
        (u) => u.name.toLowerCase().includes(search) || u.email.toLowerCase().includes(search),
      );
    }

    // Apply status filter
    if (statusFilter) {
      users = users.filter((u) => u.status === statusFilter);
    }

    // Apply sorting
    if (sortColumn && sortDirection) {
      users = [...users].sort((a, b) => {
        const aVal = a[sortColumn as keyof User];
        const bVal = b[sortColumn as keyof User];
        const comparison = aVal > bVal ? 1 : aVal < bVal ? -1 : 0;
        return sortDirection === 'asc' ? comparison : -comparison;
      });
    }

    const total = users.length;
    const start = (page - 1) * limit;
    const end = start + limit;
    const data = users.slice(start, end);

    return of({
      data,
      total,
      page,
      pageSize: limit,
    }).pipe(delay(500)); // Simulate network delay
  }

  private getUserFixtures(): User[] {
    // Deterministic pattern using seeded pseudo-random distribution
    // This ensures consistent, realistic-looking data across calls
    // Seed-based approach: use user ID as seed for deterministic "randomness"
    const users: User[] = [];

    // Predefined distribution pattern for realistic role distribution
    // More Users, fewer Admins/Managers
    const roleDistribution = [
      'User',
      'User',
      'User',
      'User',
      'User', // 5 Users
      'Editor',
      'Editor',
      'Editor', // 3 Editors
      'Viewer',
      'Viewer',
      'Viewer', // 3 Viewers
      'Manager',
      'Manager', // 2 Managers
      'Admin', // 1 Admin
    ];

    // Predefined distribution pattern for realistic status distribution
    // More active users, fewer inactive/pending
    const statusDistribution: Array<'active' | 'inactive' | 'pending'> = [
      'active',
      'active',
      'active',
      'active',
      'active',
      'active',
      'active', // 7 active
      'pending',
      'pending', // 2 pending
      'inactive', // 1 inactive
    ];

    for (let i = 1; i <= 50; i++) {
      // Use seeded selection for deterministic but varied distribution
      const roleSeed = (i * 7 + 13) % roleDistribution.length;
      const statusSeed = (i * 11 + 17) % statusDistribution.length;

      // Deterministic date generation: distribute dates evenly across 2023
      // Using i as seed ensures consistent dates for each user
      const month = (i - 1) % 12;
      const day = ((i - 1) % 28) + 1; // Days 1-28 to avoid month boundary issues

      users.push({
        id: i,
        name: `User ${i}`,
        email: `user${i}@example.com`,
        role: roleDistribution[roleSeed],
        status: statusDistribution[statusSeed],
        joinDate: new Date(2023, month, day).toISOString().split('T')[0],
      });
    }

    return users;
  }
}
