<script setup lang="ts">
import { computed } from 'vue';
import type { Note } from '../types';
import { Clock, Edit3, Trash2, Eye } from 'lucide-vue-next';

const props = withDefaults(
  defineProps<{
    note: Note;
    viewMode?: 'grid' | 'list';
  }>(),
  {
    viewMode: 'grid',
  }
);

const emit = defineEmits<{
  (e: 'view', note: Note): void;
  (e: 'edit', note: Note): void;
  (e: 'delete', note: Note): void;
}>();

const formattedCreated = computed(() => {
  if (!props.note.createdAt) return '';
  const d = new Date(props.note.createdAt);
  return d.toLocaleDateString(undefined, {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
});
</script>

<template>
  <div
    v-if="viewMode === 'list'"
    class="group bg-white border border-slate-200/90 rounded-xl p-4 flex flex-col sm:flex-row sm:items-center justify-between gap-3.5 transition-all duration-200 hover:border-blue-400 hover:shadow-md"
  >
    <div class="flex-1 min-w-0 pr-2">
      <h3
        @click="emit('view', note)"
        class="font-bold text-slate-900 text-base sm:text-lg hover:text-blue-600 cursor-pointer transition-colors truncate"
      >
        {{ note.title }}
      </h3>
      <p
        @click="emit('view', note)"
        class="text-sm sm:text-[15px] text-slate-500 truncate cursor-pointer mt-1"
      >
        {{ note.content || 'No content provided.' }}
      </p>
    </div>

    <div class="flex items-center justify-between sm:justify-end gap-4 shrink-0 pt-2 sm:pt-0 border-t sm:border-t-0 border-slate-100">
      <div class="flex flex-col sm:items-end text-xs text-slate-400 leading-tight">
        <div class="flex items-center gap-1.5">
          <Clock class="w-3.5 h-3.5 text-slate-400" />
          <span>{{ formattedCreated }}</span>
        </div>
      </div>

      <div class="flex items-center gap-1">
        <button
          @click.stop="emit('view', note)"
          title="View Note"
          class="p-1.5 text-slate-500 hover:text-blue-600 hover:bg-blue-50 rounded-lg transition-colors cursor-pointer"
        >
          <Eye class="w-4 h-4" />
        </button>
        <button
          @click.stop="emit('edit', note)"
          title="Edit Note"
          class="p-1.5 text-slate-500 hover:text-amber-600 hover:bg-amber-50 rounded-lg transition-colors cursor-pointer"
        >
          <Edit3 class="w-4 h-4" />
        </button>
        <button
          @click.stop="emit('delete', note)"
          title="Delete Note"
          class="p-1.5 text-slate-500 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors cursor-pointer"
        >
          <Trash2 class="w-4 h-4" />
        </button>
      </div>
    </div>
  </div>

  <div
    v-else
    @click="emit('view', note)"
    class="group relative bg-white border border-slate-200/90 rounded-2xl p-5 flex flex-col justify-between transition-all duration-200 hover:shadow-lg hover:border-blue-400 hover:-translate-y-0.5"
  >
    <div>
      <h3
        class="font-bold text-slate-900 text-lg leading-snug hover:text-blue-600 cursor-pointer transition-colors line-clamp-2 mb-2"
      >
        {{ note.title }}
      </h3>

      <p
        class="text-slate-500 text-sm sm:text-[15px] leading-relaxed line-clamp-4 cursor-pointer whitespace-pre-line mb-5"
      >
        {{ note.content || 'No content provided.' }}
      </p>
    </div>

    <div
      @click.stop
      class="pt-3.5 border-t border-slate-100 flex items-center justify-between gap-2 mt-auto"
    >
      <div class="flex items-center gap-1.5 text-xs text-slate-400">
        <Clock class="w-3.5 h-3.5 text-slate-400" />
        <span>{{ formattedCreated }}</span>
      </div>

      <div class="flex items-center gap-1">
        <button
          @click.stop="emit('view', note)"
          title="View Details"
          class="p-1.5 text-slate-400 hover:text-blue-600 hover:bg-blue-50 rounded-lg transition-colors cursor-pointer"
        >
          <Eye class="w-4 h-4" />
        </button>
        <button
          @click.stop="emit('edit', note)"
          title="Edit Note"
          class="p-1.5 text-slate-400 hover:text-amber-600 hover:bg-amber-50 rounded-lg transition-colors cursor-pointer"
        >
          <Edit3 class="w-4 h-4" />
        </button>
        <button
          @click.stop="emit('delete', note)"
          title="Delete Note"
          class="p-1.5 text-slate-400 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors cursor-pointer"
        >
          <Trash2 class="w-4 h-4" />
        </button>
      </div>
    </div>
  </div>
</template>
