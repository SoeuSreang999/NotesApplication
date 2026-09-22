export interface User {
  id: number;
  username: string;
  email: string;
  createdAt: string;
}

export interface Note {
  id: number;
  userId: number;
  title: string;
  content?: string | null;
  createdAt: string;
  updatedAt?: string | null;
  updatedBy?: number | null;
}

export interface AuthResponse {
  message: string;
  token: string;
  user: User;
}

export interface CreateNotePayload {
  title: string;
  content?: string | null;
}

export interface UpdateNotePayload {
  title: string;
  content?: string | null;
}

export interface PaginatedResponse<T> {
  data: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasMore: boolean;
}
