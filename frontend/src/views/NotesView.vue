<script setup lang="ts">
import { ref, computed, watch, onMounted, onUnmounted } from 'vue';
import { useNotesStore } from '../stores/notes';
import Navbar from '../components/Navbar.vue';
import NoteCard from '../components/NoteCard.vue';
import NoteModal from '../components/NoteModal.vue';
import DeleteModal from '../components/DeleteModal.vue';
import type { Note } from '../types';
import {
  Plus,
  Search,
  Loader2,
  ArrowUpDown,
  ChevronDown,
  Check,
  LayoutGrid,
  List,
} from 'lucide-vue-next';

const notesStore = useNotesStore();

const viewMode = ref<'grid' | 'list'>(
  (localStorage.getItem('notes_view_mode') as 'grid' | 'list') || 'grid'
);

const setViewMode = (mode: 'grid' | 'list') => {
  viewMode.value = mode;
  localStorage.setItem('notes_view_mode', mode);
};

const sortOptions = [
  { value: 'latest', label: 'Newest' },
  { value: 'oldest', label: 'Oldest' },
  { value: 'title_asc', label: 'Title (A - Z)' },
  { value: 'title_desc', label: 'Title (Z - A)' },
];

const currentSortLabel = computed(() => {
  return sortOptions.find(opt => opt.value === notesStore.sortBy)?.label || 'Newest';
});

const isSortOpen = ref(false);
const sortRef = ref<HTMLElement | null>(null);

const toggleSort = () => {
  isSortOpen.value = !isSortOpen.value;
};

const selectSort = (val: any) => {
  notesStore.sortBy = val;
  isSortOpen.value = false;
};

const handleSortClickOutside = (event: MouseEvent) => {
  if (sortRef.value && !sortRef.value.contains(event.target as Node)) {
    isSortOpen.value = false;
  }
};

let searchDebounceTimer: any = null;
watch(
  () => notesStore.searchQuery,
  () => {
    clearTimeout(searchDebounceTimer);
    searchDebounceTimer = setTimeout(() => {
      notesStore.fetchNotes(true);
    }, 300);
  }
);

watch(
  () => notesStore.sortBy,
  () => {
    notesStore.fetchNotes(true);
  }
);

const isNoteModalOpen = ref(false);
const noteModalMode = ref<'create' | 'edit' | 'view'>('create');
const currentNote = ref<Note | null>(null);

const isDeleteModalOpen = ref(false);
const noteToDelete = ref<Note | null>(null);

onMounted(() => {
  notesStore.fetchNotes(true);
  document.addEventListener('click', handleSortClickOutside);
});

onUnmounted(() => {
  document.removeEventListener('click', handleSortClickOutside);
});

const modalError = ref<string | null>(null);

const openCreateModal = () => {
  noteModalMode.value = 'create';
  currentNote.value = null;
  modalError.value = null;
  isNoteModalOpen.value = true;
};

const handleViewNote = (note: Note) => {
  isDeleteModalOpen.value = false;
  currentNote.value = note;
  noteModalMode.value = 'view';
  modalError.value = null;
  isNoteModalOpen.value = true;
};

const handleEditNote = (note: Note) => {
  isDeleteModalOpen.value = false;
  currentNote.value = note;
  noteModalMode.value = 'edit';
  modalError.value = null;
  isNoteModalOpen.value = true;
};

const handleSwitchToEdit = () => {
  modalError.value = null;
  noteModalMode.value = 'edit';
};

const handleNoteSubmit = async (data: { title: string; content: string }) => {
  modalError.value = null;
  try {
    if (noteModalMode.value === 'create') {
      await notesStore.createNote(data);
    } else if (noteModalMode.value === 'edit' && currentNote.value) {
      await notesStore.updateNote(currentNote.value.id, data);
    }
    isNoteModalOpen.value = false;
  } catch (err: any) {
    modalError.value = err.message || 'Operation failed';
  }
};

const handleDeletePrompt = (note: Note) => {
  isNoteModalOpen.value = false;
  noteToDelete.value = note;
  isDeleteModalOpen.value = true;
};

const handleConfirmDelete = async () => {
  if (!noteToDelete.value) return;
  try {
    await notesStore.deleteNote(noteToDelete.value.id);
    isDeleteModalOpen.value = false;
    noteToDelete.value = null;
  } catch (err) {
    
  }
};
</script>

<template>
  <div class="min-h-screen bg-slate-50/60 text-slate-900 flex flex-col relative">
    <Navbar />

    <main class="flex-1 max-w-7xl w-full mx-auto px-4 sm:px-6 lg:px-8 pb-12">
      <div class="sticky top-16 z-30 bg-slate-50/95 backdrop-blur-md pt-6 pb-4 mb-6 -mx-4 px-4 sm:-mx-6 sm:px-6 lg:-mx-8 lg:px-8 transition-all">
        <div class="mb-4">
          <h1 class="text-2xl sm:text-3xl font-extrabold text-slate-900 tracking-tight my-0">
            My Notes
          </h1>
          <p class="text-xs sm:text-sm text-slate-500 mt-1">
            Organize, capture, and manage your personal thoughts and tasks.
          </p>
        </div>

        <div class="flex flex-col md:flex-row md:items-center justify-between gap-3.5">
          <div class="relative flex-1 max-w-lg">
            <Search class="w-4 h-4 text-slate-400 absolute left-3.5 top-1/2 -translate-y-1/2 pointer-events-none" />
            <input
              v-model="notesStore.searchQuery"
              type="text"
              placeholder="Search notes by title or content..."
              class="w-full pl-10 pr-4 py-2 bg-white border border-slate-200/90 rounded-full text-xs sm:text-sm text-slate-800 placeholder-slate-400 focus:bg-white focus:outline-none focus:border-blue-600 focus:ring-1 focus:ring-blue-600 shadow-2xs transition-all"
            />
          </div>

          <div class="flex items-center gap-2.5 flex-wrap sm:flex-nowrap">
            <div class="relative" ref="sortRef">
              <button
                @click.stop="toggleSort"
                type="button"
                class="flex items-center gap-2 bg-white border border-slate-200/90 hover:border-slate-300 rounded-full px-3.5 h-9 shadow-2xs text-xs sm:text-sm font-medium text-slate-700 transition-all cursor-pointer focus:outline-none"
              >
                <ArrowUpDown class="w-3.5 h-3.5 text-slate-500" />
                <span class="text-slate-400 font-normal hidden sm:inline">Sort:</span>
                <span class="text-slate-800 font-semibold">{{ currentSortLabel }}</span>
                <ChevronDown
                  class="w-3.5 h-3.5 text-slate-400 transition-transform duration-150"
                  :class="{ 'rotate-180': isSortOpen }"
                />
              </button>

              <div
                v-if="isSortOpen"
                class="absolute right-0 mt-2 w-44 bg-white border border-slate-200/90 rounded-2xl shadow-xl p-1.5 z-40 animate-in fade-in duration-100"
              >
                <div class="space-y-0.5">
                  <button
                    v-for="opt in sortOptions"
                    :key="opt.value"
                    @click="selectSort(opt.value)"
                    class="w-full flex items-center justify-between px-3 py-2 text-xs sm:text-sm rounded-xl transition-colors cursor-pointer text-left"
                    :class="notesStore.sortBy === opt.value ? 'bg-blue-50 text-blue-700 font-semibold' : 'text-slate-700 hover:bg-slate-50 font-medium'"
                  >
                    <span>{{ opt.label }}</span>
                    <Check v-if="notesStore.sortBy === opt.value" class="w-4 h-4 text-blue-600 shrink-0" />
                  </button>
                </div>
              </div>
            </div>

            <div class="flex items-center bg-slate-200/60 border border-slate-200 rounded-full p-0.5 shadow-2xs">
              <button
                @click="setViewMode('grid')"
                title="Grid View"
                class="p-1.5 rounded-full transition-all cursor-pointer"
                :class="viewMode === 'grid' ? 'bg-white text-slate-900 shadow-xs' : 'text-slate-500 hover:text-slate-800'"
              >
                <LayoutGrid class="w-3.5 h-3.5" />
              </button>
              <button
                @click="setViewMode('list')"
                title="List View"
                class="p-1.5 rounded-full transition-all cursor-pointer"
                :class="viewMode === 'list' ? 'bg-white text-slate-900 shadow-xs' : 'text-slate-500 hover:text-slate-800'"
              >
                <List class="w-3.5 h-3.5" />
              </button>
            </div>

            <button
              @click="openCreateModal"
              class="inline-flex items-center gap-2 px-4 py-2 bg-[#0f3a5f] hover:bg-[#0a2540] text-white text-xs sm:text-sm font-medium rounded-full shadow-sm hover:shadow-md transition-all duration-200 active:scale-95 cursor-pointer shrink-0"
            >
              <Plus class="w-4 h-4" />
              <span>New Note</span>
            </button>
          </div>
        </div>
      </div>

      <div v-if="notesStore.loading && notesStore.notes.length === 0" class="py-24 flex flex-col items-center justify-center">
        <Loader2 class="w-8 h-8 text-blue-600 animate-spin mb-3" />
        <p class="text-sm text-slate-500">Loading your notes...</p>
      </div>

      <div
        v-else-if="notesStore.error"
        class="p-6 bg-red-50 border border-red-200 rounded-2xl text-center max-w-md mx-auto my-12"
      >
        <p class="text-red-700 text-sm font-semibold mb-3">{{ notesStore.error }}</p>
        <button
          @click="notesStore.fetchNotes(true)"
          class="px-5 py-2 bg-red-600 text-white text-xs font-medium rounded-full shadow-xs hover:bg-red-700 transition-colors cursor-pointer"
        >
          Try Again
        </button>
      </div>

      <div
        v-else-if="notesStore.filteredNotes.length === 0"
        class="py-24 text-center flex flex-col items-center justify-center"
      >
        <p class="text-sm font-medium text-slate-400">
          Data Not Found
        </p>
      </div>

      <div v-else class="space-y-8">
        <div
          :class="{
            'grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-5': viewMode === 'grid',
            'flex flex-col gap-3': viewMode === 'list',
          }"
        >
          <NoteCard
            v-for="note in notesStore.filteredNotes"
            :key="note.id"
            :note="note"
            :view-mode="viewMode"
            @view="handleViewNote"
            @edit="handleEditNote"
            @delete="handleDeletePrompt"
          />
        </div>

        <div v-if="notesStore.hasMore" class="pt-4 pb-8 flex flex-col items-center justify-center">
          <button
            @click="notesStore.loadMore()"
            :disabled="notesStore.loadingMore"
            class="inline-flex items-center gap-2 px-6 py-2.5 bg-white border border-slate-300 hover:border-blue-600 hover:bg-blue-50/30 text-slate-800 hover:text-blue-600 text-xs sm:text-sm font-semibold rounded-full shadow-2xs hover:shadow-md transition-all active:scale-95 cursor-pointer disabled:opacity-50"
          >
            <Loader2 v-if="notesStore.loadingMore" class="w-4 h-4 animate-spin text-blue-600" />
            <span>{{ notesStore.loadingMore ? 'Loading more...' : 'Load More' }}</span>
          </button>
          <p class="text-[11px] text-slate-400 mt-2">
            Showing {{ notesStore.notes.length }} of {{ notesStore.totalCount }} notes
          </p>
        </div>
      </div>
    </main>

    <NoteModal
      :is-open="isNoteModalOpen"
      :mode="noteModalMode"
      :note="currentNote"
      :loading="notesStore.loading"
      :error-message="modalError"
      @close="isNoteModalOpen = false"
      @submit="handleNoteSubmit"
      @switch-to-edit="handleSwitchToEdit"
    />

    <DeleteModal
      :is-open="isDeleteModalOpen"
      :title="noteToDelete?.title"
      :loading="notesStore.loading"
      @close="isDeleteModalOpen = false"
      @confirm="handleConfirmDelete"
    />
  </div>
</template>
