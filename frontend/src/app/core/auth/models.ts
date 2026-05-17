export interface AuthResponse {
  accessToken: string;
  expiresAt: string;
  userId: string;
  email: string;
}

export interface AuthUser {
  userId: string;
  email: string;
}

export interface Session {
  token: string;
  expiresAt: string;
  user: AuthUser;
}
