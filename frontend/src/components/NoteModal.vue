<script setup lang="ts">
import { ref, watch } from 'vue';
import type { Note } from '../types';
import { X, Check, Edit3 } from 'lucide-vue-next';

const props = defineProps<{
  isOpen: boolean;
  mode: 'create' | 'edit' | 'view';
  note?: Note | null;
  loading?: boolean;
  errorMessage?: string | null;
}>();

const emit = defineEmits<{
  (e: 'close'): void;
  (e: 'submit', data: { title: string; content: string }): void;
  (e: 'switch-to-edit'): void;
}>();

const title = ref('');
const content = ref('');
const error = ref('');

watch(
  () => [props.isOpen, props.note],
  () => {
    if (props.note && (props.mode === 'edit' || props.mode === 'view')) {
      title.value = props.note.title;
      content.value = props.note.content || '';
    } else {
      title.value = '';
      content.value = '';
    }
    error.value = '';
  },
  { immediate: true }
);

watch(
  () => props.errorMessage,
  (newVal) => {
    if (newVal) {
      error.value = newVal;
    }
  }
);

watch(title, () => {
  if (error.value) {
    error.value = '';
  }
});

const handleSubmit = () => {
  const trimmedTitle = title.value.trim();
  if (!trimmedTitle) {
    error.value = 'Title is mandatory.';
    return;
  }
  if (trimmedTitle.length > 255) {
    error.value = 'Title cannot exceed 255 characters.';
    return;
  }
  error.value = '';
  emit('submit', {
    title: trimmedTitle,
    content: content.value.trim(),
  });
};
</script>

<template>
  <Transition name="modal-fade">
    <div v-if="isOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4">
      <div
        class="fixed inset-0 bg-black/50"
        @click="emit('close')"
      ></div>

      <div
        class="modal-content relative z-10 grid w-full max-w-xl gap-4 border border-slate-200/90 bg-white p-6 sm:p-7 shadow-2xl rounded-2xl max-h-[90vh] overflow-y-auto"
      >
      <button
        @click="emit('close')"
        class="absolute right-4 top-4 p-1.5 rounded-full text-slate-400 hover:text-slate-700 hover:bg-slate-100 transition-colors cursor-pointer"
      >
        <X class="w-4 h-4" />
      </button>

      <div class="flex flex-col space-y-1 text-left pb-3 border-b border-slate-100">
        <h3 class="text-xl font-bold text-slate-900">
          {{ mode === 'create' ? 'Create Note' : mode === 'edit' ? 'Edit Note' : 'Note Details' }}
        </h3>
      </div>

      <template v-if="mode === 'view'">
        <div class="space-y-3 py-1">
          <h2 class="text-2xl font-bold tracking-tight text-slate-900 leading-snug">
            {{ title }}
          </h2>

          <div class="text-slate-700 text-base leading-relaxed whitespace-pre-wrap min-h-[120px] pt-1">
            {{ content || 'No content written for this note.' }}
          </div>
        </div>
      </template>

      <template v-else>
        <div class="space-y-4 py-1">
          <div class="space-y-1.5 text-left">
            <label class="text-sm font-semibold text-slate-700 leading-none">
              Title <span class="text-red-500">*</span>
            </label>
            <input
              v-model="title"
              type="text"
              maxlength="255"
              placeholder="e.g. Project Roadmap or Meeting Notes"
              class="flex h-11 w-full rounded-xl border bg-slate-50/60 px-3.5 py-1 text-base text-slate-900 placeholder:text-slate-400 focus:bg-white focus:outline-none transition-colors"
              :class="error ? 'border-red-500 focus:border-red-600 focus:ring-1 focus:ring-red-600' : 'border-slate-200 focus:border-blue-600 focus:ring-1 focus:ring-blue-600'"
              autofocus
            />
            <span v-if="error" class="text-xs text-red-600 font-medium block pt-0.5">
              {{ error }}
            </span>
          </div>

          <div class="space-y-1.5 text-left">
            <label class="text-sm font-semibold text-slate-700 leading-none">
              Content <span class="text-xs text-slate-400 font-normal">(Optional)</span>
            </label>
            <textarea
              v-model="content"
              rows="7"
              placeholder="Write your note thoughts here..."
              class="flex min-h-[140px] w-full rounded-xl border border-slate-200 bg-slate-50/60 p-3.5 text-base text-slate-900 placeholder:text-slate-400 focus:bg-white focus:outline-none focus:border-blue-600 focus:ring-1 focus:ring-blue-600 transition-colors resize-y"
            ></textarea>
          </div>
        </div>
      </template>

      <div class="flex flex-col-reverse sm:flex-row sm:justify-end sm:items-center pt-3 border-t border-slate-100 gap-3">
        <div class="flex items-center justify-end gap-2.5">
          <button
            @click="emit('close')"
            class="inline-flex items-center justify-center rounded-full text-sm font-medium border border-slate-200 bg-white hover:bg-slate-50 h-10 px-4.5 text-slate-700 transition-colors cursor-pointer"
          >
            {{ mode === 'view' ? 'Close' : 'Cancel' }}
          </button>

          <button
            v-if="mode === 'view'"
            @click="emit('switch-to-edit')"
            class="inline-flex items-center gap-1.5 justify-center rounded-full text-sm font-medium bg-[#0f3a5f] hover:bg-[#0a2540] text-white h-10 px-4.5 shadow-sm transition-all active:scale-95 cursor-pointer"
          >
            <Edit3 class="w-4 h-4" />
            <span>Edit Note</span>
          </button>

          <button
            v-else
            @click="handleSubmit"
            :disabled="loading"
            class="inline-flex items-center gap-1.5 justify-center rounded-full text-sm font-medium bg-[#0f3a5f] hover:bg-[#0a2540] text-white disabled:opacity-50 h-10 px-5 shadow-sm transition-all active:scale-95 cursor-pointer"
          >
            <Check class="w-4 h-4" />
            <span>{{ loading ? 'Saving...' : mode === 'create' ? 'Create Note' : 'Save Changes' }}</span>
          </button>
        </div>
      </div>
    </div>
  </div>
  </Transition>
</template>
