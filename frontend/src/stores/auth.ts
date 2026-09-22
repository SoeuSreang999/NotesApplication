import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import api from '../api/client';
import type { User, AuthResponse } from '../types';

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string | null>(localStorage.getItem('notes_token'));
  const user = ref<User | null>(
    localStorage.getItem('notes_user')
      ? JSON.parse(localStorage.getItem('notes_user')!)
      : null
  );
  const loading = ref(false);
  const error = ref<string | null>(null);

  const isAuthenticated = computed(() => !!token.value);

  const setAuth = (authData: AuthResponse) => {
    token.value = authData.token;
    user.value = authData.user;
    localStorage.setItem('notes_token', authData.token);
    localStorage.setItem('notes_user', JSON.stringify(authData.user));
  };

  const login = async (username: string, password: string) => {
    loading.value = true;
    error.value = null;
    try {
      const response = await api.post<AuthResponse>('/auth/login', { username, password });
      setAuth(response.data);
      return response.data;
    } catch (err: any) {
      const message = err.response?.data?.message || err.response?.data?.detail || 'Failed to login. Please check your credentials.';
      error.value = message;
      throw new Error(message);
    } finally {
      loading.value = false;
    }
  };

  const register = async (username: string, email: string, password: string) => {
    loading.value = true;
    error.value = null;
    try {
      const response = await api.post<AuthResponse>('/auth/register', { username, email, password });
      setAuth(response.data);
      return response.data;
    } catch (err: any) {
      const message = err.response?.data?.message || err.response?.data?.detail || 'Registration failed. Please try again.';
      error.value = message;
      throw new Error(message);
    } finally {
      loading.value = false;
    }
  };

  const updateProfile = async (data: { username?: string; password?: string }) => {
    loading.value = true;
    error.value = null;
    try {
      const response = await api.put<{ message: string; user: User }>('/users/profile', data);
      user.value = response.data.user;
      localStorage.setItem('notes_user', JSON.stringify(response.data.user));
      return response.data;
    } catch (err: any) {
      const message = err.response?.data?.message || err.response?.data?.detail || 'Failed to update profile.';
      error.value = message;
      throw new Error(message);
    } finally {
      loading.value = false;
    }
  };

  const logout = () => {
    token.value = null;
    user.value = null;
    localStorage.removeItem('notes_token');
    localStorage.removeItem('notes_user');
  };

  return {
    token,
    user,
    loading,
    error,
    isAuthenticated,
    login,
    register,
    updateProfile,
    logout,
  };
});
