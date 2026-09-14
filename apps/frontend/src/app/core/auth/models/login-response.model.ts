import type { User } from '@core/auth/models/user.model';

export interface LoginResponse {
  token: string;
  user: User;
}
