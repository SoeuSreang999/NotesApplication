<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '../stores/auth';
import { FileText, Lock, User, ArrowRight } from 'lucide-vue-next';

const router = useRouter();
const authStore = useAuthStore();

const username = ref('');
const password = ref('');
const localError = ref('');

const handleLogin = async () => {
  if (!username.value.trim() || !password.value) {
    localError.value = 'Please enter your username/email and password.';
    return;
  }
  localError.value = '';

  try {
    await authStore.login(username.value.trim(), password.value);
    router.push('/');
  } catch (err: any) {
    localError.value = err.message || 'Login failed';
  }
};
</script>

<template>
  <div class="min-h-screen bg-slate-50 flex flex-col justify-center py-12 px-4 sm:px-6 lg:px-8 relative overflow-hidden">
    <div class="absolute -top-32 -left-32 w-80 h-80 bg-blue-100 rounded-full blur-3xl pointer-events-none opacity-70"></div>
    <div class="absolute -bottom-32 -right-32 w-80 h-80 bg-slate-200 rounded-full blur-3xl pointer-events-none opacity-60"></div>

    <div class="sm:mx-auto sm:w-full sm:max-w-md relative z-10">
      <div class="flex justify-center mb-3.5">
        <div class="w-11 h-11 rounded-xl bg-blue-600 flex items-center justify-center text-white shadow-md shadow-blue-500/25">
          <FileText class="w-5 h-5 text-white" />
        </div>
      </div>
      <h2 class="text-center text-2xl font-extrabold tracking-tight text-slate-900">
        Sign in to Notes Application
      </h2>
      <p class="mt-1 text-center text-xs sm:text-sm text-slate-500">
        Access your notes workspace securely
      </p>
    </div>

    <div class="mt-7 sm:mx-auto sm:w-full sm:max-w-md relative z-10">
      <div class="bg-white border border-slate-200/90 rounded-2xl p-7 shadow-xl shadow-slate-200/50">
        <div
          v-if="localError || authStore.error"
          class="mb-4 p-3 bg-red-50 border border-red-200 rounded-xl text-red-700 text-xs font-medium"
        >
          {{ localError || authStore.error }}
        </div>

        <form @submit.prevent="handleLogin" class="space-y-4">
          <div class="space-y-1 text-left">
            <label class="text-xs font-semibold text-slate-700">
              Username or Email
            </label>
            <div class="relative">
              <User class="w-4 h-4 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2 pointer-events-none" />
              <input
                v-model="username"
                type="text"
                required
                placeholder="e.g. sreang or user@example.com"
                class="flex h-10 w-full rounded-xl border border-slate-200 bg-slate-50/60 pl-9 pr-3.5 text-xs sm:text-sm text-slate-900 placeholder:text-slate-400 focus:bg-white focus:outline-none focus:border-blue-600 focus:ring-1 focus:ring-blue-600 transition-colors"
              />
            </div>
          </div>

          <div class="space-y-1 text-left">
            <label class="text-xs font-semibold text-slate-700">
              Password
            </label>
            <div class="relative">
              <Lock class="w-4 h-4 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2 pointer-events-none" />
              <input
                v-model="password"
                type="password"
                required
                placeholder="••••••••"
                class="flex h-10 w-full rounded-xl border border-slate-200 bg-slate-50/60 pl-9 pr-3.5 text-xs sm:text-sm text-slate-900 placeholder:text-slate-400 focus:bg-white focus:outline-none focus:border-blue-600 focus:ring-1 focus:ring-blue-600 transition-colors"
              />
            </div>
          </div>

          <div class="pt-2">
            <button
              type="submit"
              :disabled="authStore.loading"
              class="inline-flex items-center justify-center gap-2 w-full h-10 rounded-full text-xs sm:text-sm font-medium bg-[#0f3a5f] hover:bg-[#0a2540] text-white disabled:opacity-50 transition-all shadow-sm hover:shadow-md active:scale-95 cursor-pointer"
            >
              <span>{{ authStore.loading ? 'Signing in...' : 'Sign In' }}</span>
              <ArrowRight class="w-4 h-4" />
            </button>
          </div>
        </form>

        <div class="mt-5 pt-3.5 border-t border-slate-100 text-center text-xs text-slate-500">
          Don't have an account?
          <RouterLink to="/register" class="font-semibold text-blue-600 hover:text-blue-700 ml-1">
            Create account
          </RouterLink>
        </div>
      </div>
    </div>
  </div>
</template>
