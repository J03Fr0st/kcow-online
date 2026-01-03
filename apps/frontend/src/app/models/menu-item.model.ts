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
    label: 'Schools',
    icon: 'school',
    route: '/schools',
  },
  {
    label: 'Students',
    icon: 'students',
    route: '/students',
  },
  {
    label: 'Class Groups',
    icon: 'class-groups',
    route: '/class-groups',
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
  },
  {
    label: 'Import',
    icon: 'import',
    route: '/import',
  },
  {
    label: 'Workspace Settings',
    icon: 'settings',
    route: '/workspace-settings',
  },
];
