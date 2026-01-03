export type StatIcon = 'dollar' | 'users' | 'shopping' | 'trending';

export interface StatCard {
  title: string;
  value: string | number;
  change: number;
  changeType: 'increase' | 'decrease';
  icon: StatIcon;
  color: 'primary' | 'secondary' | 'accent' | 'success' | 'warning' | 'error' | 'info';
}
