import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import api from '../api/client';
import type { Note, CreateNotePayload, UpdateNotePayload, PaginatedResponse } from '../types';

export const useNotesStore = defineStore('notes', () => {
  const notes = ref<Note[]>([]);
  const loading = ref(false);
  const loadingMore = ref(false);
  const error = ref<string | null>(null);
  const searchQuery = ref('');
  const sortBy = ref<'latest' | 'oldest' | 'title_asc' | 'title_desc'>('latest');

  const page = ref(1);
  const pageSize = ref(25);
  const totalCount = ref(0);
  const hasMore = ref(false);

  const fetchNotes = async (reset = true) => {
    if (reset) {
      page.value = 1;
      loading.value = true;
    }
    error.value = null;

    try {
      const response = await api.get<PaginatedResponse<Note> | Note[]>('/notes', {
        params: {
          search: searchQuery.value.trim() || undefined,
          sort: sortBy.value,
          page: page.value,
          pageSize: pageSize.value,
        },
      });

      const resData = response.data;
      if (resData && !Array.isArray(resData) && 'data' in resData) {
        const items = resData.data;
        totalCount.value = resData.totalCount;
        hasMore.value = resData.hasMore;
        if (reset) {
          notes.value = items;
        } else {
          notes.value.push(...items);
        }
      } else if (Array.isArray(resData)) {
        totalCount.value = resData.length;
        hasMore.value = resData.length === pageSize.value;
        if (reset) {
          notes.value = resData;
        } else {
          notes.value.push(...resData);
        }
      }
    } catch (err: any) {
      error.value = err.response?.data?.message || 'Failed to fetch notes';
    } finally {
      loading.value = false;
      loadingMore.value = false;
    }
  };

  const loadMore = async () => {
    if (loading.value || loadingMore.value || !hasMore.value) return;
    loadingMore.value = true;
    page.value += 1;
    await fetchNotes(false);
  };

  const createNote = async (payload: CreateNotePayload) => {
    loading.value = true;
    error.value = null;
    try {
      const response = await api.post<Note>('/notes', payload);
      notes.value.unshift(response.data);
      totalCount.value += 1;
      return response.data;
    } catch (err: any) {
      const msg = err.response?.data?.message || 'Failed to create note';
      error.value = msg;
      throw new Error(msg);
    } finally {
      loading.value = false;
    }
  };

  const updateNote = async (id: number, payload: UpdateNotePayload) => {
    loading.value = true;
    error.value = null;
    try {
      const response = await api.put<Note>(`/notes/${id}`, payload);
      const index = notes.value.findIndex((n) => n.id === id);
      if (index !== -1) {
        notes.value[index] = response.data;
      }
      return response.data;
    } catch (err: any) {
      const msg = err.response?.data?.message || 'Failed to update note';
      error.value = msg;
      throw new Error(msg);
    } finally {
      loading.value = false;
    }
  };

  const deleteNote = async (id: number) => {
    loading.value = true;
    error.value = null;
    try {
      await api.delete(`/notes/${id}`);
      notes.value = notes.value.filter((n) => n.id !== id);
      if (totalCount.value > 0) totalCount.value -= 1;
    } catch (err: any) {
      const msg = err.response?.data?.message || 'Failed to delete note';
      error.value = msg;
      throw new Error(msg);
    } finally {
      loading.value = false;
    }
  };

  const filteredNotes = computed(() => notes.value);

  return {
    notes,
    filteredNotes,
    loading,
    loadingMore,
    error,
    searchQuery,
    sortBy,
    page,
    pageSize,
    totalCount,
    hasMore,
    fetchNotes,
    loadMore,
    createNote,
    updateNote,
    deleteNote,
  };
});
