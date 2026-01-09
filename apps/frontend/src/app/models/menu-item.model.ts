export interface MenuItem {
  label: string;
  icon: string;
  route: string;
  badge?: number;
  children?: MenuItem[];
}

export const MENU_ITEMS: MenuItem[] = [
  {
    label: 'Dashboard',
    icon: 'home',
    route: '/dashboard',
  },
  {
    label: 'Trucks',
    icon: 'truck',
    route: '/trucks',
  },
  {
    label: 'Schools',
    icon: 'school',
    route: '/schools',
  },
  {
    label: 'Class Groups',
    icon: 'class-groups',
    route: '/class-groups',
  },
  {
    label: 'Students',
    icon: 'students',
    route: '/students',
  },
  {
    label: 'Activities',
    icon: 'activities',
    route: '/activities',
  },
  {
    label: 'Attendance',
    icon: 'attendance',
    route: '/attendance',
  },
  {
    label: 'Evaluations',
    icon: 'evaluations',
    route: '/evaluations',
  },
  {
    label: 'Billing',
    icon: 'billing',
    route: '/billing',
  }
];
