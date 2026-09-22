<script setup lang="ts">
import { AlertTriangle, Trash2 } from 'lucide-vue-next';

defineProps<{
  isOpen: boolean;
  title?: string;
  loading?: boolean;
}>();

const emit = defineEmits<{
  (e: 'close'): void;
  (e: 'confirm'): void;
}>();
</script>

<template>
  <Transition name="modal-fade">
    <div v-if="isOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4">
      <div
        class="fixed inset-0 bg-black/50"
        @click="emit('close')"
      ></div>

      <div
        class="modal-content relative z-10 grid w-full max-w-md gap-4 border border-slate-200 bg-white p-6 sm:p-7 shadow-2xl rounded-2xl"
      >
      <div class="flex items-start gap-3.5">
        <div class="w-10 h-10 rounded-xl bg-red-50 border border-red-100 flex items-center justify-center text-red-600 shrink-0">
          <AlertTriangle class="w-5 h-5" />
        </div>
        <div class="flex flex-col space-y-1.5 text-left">
          <h3 class="text-lg font-bold text-slate-900 leading-none">
            Delete Note
          </h3>
          <p class="text-sm text-slate-500">
            This action cannot be undone. This note will be permanently removed.
          </p>
        </div>
      </div>

      <div class="rounded-xl border border-slate-200/80 bg-slate-50/70 p-4 text-sm sm:text-base text-slate-700">
        Are you sure you want to delete <span class="font-bold text-slate-900">"{{ title }}"</span>?
      </div>

      <div class="flex items-center justify-end gap-2.5 pt-2">
        <button
          @click="emit('close')"
          class="inline-flex items-center justify-center rounded-full text-sm font-medium border border-slate-200 bg-white hover:bg-slate-50 h-10 px-4.5 text-slate-700 transition-colors cursor-pointer"
        >
          Cancel
        </button>

        <button
          @click="emit('confirm')"
          :disabled="loading"
          class="inline-flex items-center gap-1.5 justify-center rounded-full text-sm font-medium bg-red-600 text-white hover:bg-red-700 disabled:opacity-50 h-10 px-5 shadow-sm transition-all active:scale-95 cursor-pointer"
        >
          <Trash2 class="w-4 h-4" />
          <span>{{ loading ? 'Deleting...' : 'Delete' }}</span>
        </button>
      </div>
    </div>
  </div>
  </Transition>
</template>
