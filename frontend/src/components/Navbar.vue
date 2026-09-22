<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '../stores/auth';
import { LogOut, ChevronDown, User, Mail, Calendar, X, Edit3, Check } from 'lucide-vue-next';

const router = useRouter();
const authStore = useAuthStore();

const isDropdownOpen = ref(false);
const isProfileModalOpen = ref(false);
const isEditingProfile = ref(false);
const editUsername = ref('');
const editPassword = ref('');
const editConfirmPassword = ref('');
const profileError = ref<string | null>(null);
const profileSuccess = ref<string | null>(null);
const dropdownRef = ref<HTMLElement | null>(null);

const toggleDropdown = () => {
  isDropdownOpen.value = !isDropdownOpen.value;
};

const closeDropdown = () => {
  isDropdownOpen.value = false;
};

const openProfile = () => {
  closeDropdown();
  isEditingProfile.value = false;
  profileError.value = null;
  profileSuccess.value = null;
  editUsername.value = authStore.user?.username || '';
  editPassword.value = '';
  editConfirmPassword.value = '';
  isProfileModalOpen.value = true;
};

const startEditProfile = () => {
  isEditingProfile.value = true;
  profileError.value = null;
  profileSuccess.value = null;
  editUsername.value = authStore.user?.username || '';
  editPassword.value = '';
  editConfirmPassword.value = '';
};

const cancelEditProfile = () => {
  isEditingProfile.value = false;
  profileError.value = null;
  profileSuccess.value = null;
  editConfirmPassword.value = '';
};

const handleSaveProfile = async () => {
  if (!editUsername.value.trim()) {
    profileError.value = 'Username cannot be empty.';
    return;
  }
  if (editPassword.value) {
    if (editPassword.value.length < 6) {
      profileError.value = 'Password must be at least 6 characters.';
      return;
    }
    if (editPassword.value !== editConfirmPassword.value) {
      profileError.value = 'Passwords do not match.';
      return;
    }
  }
  profileError.value = null;
  try {
    await authStore.updateProfile({
      username: editUsername.value.trim(),
      password: editPassword.value ? editPassword.value : undefined,
    });
    profileSuccess.value = 'Profile updated successfully!';
    setTimeout(() => {
      isEditingProfile.value = false;
      profileSuccess.value = null;
    }, 1200);
  } catch (err: any) {
    profileError.value = err.message || 'Failed to update profile.';
  }
};

const handleLogout = () => {
  closeDropdown();
  authStore.logout();
  router.push('/login');
};

const handleClickOutside = (event: MouseEvent) => {
  if (dropdownRef.value && !dropdownRef.value.contains(event.target as Node)) {
    closeDropdown();
  }
};

onMounted(() => {
  document.addEventListener('click', handleClickOutside);
});

onUnmounted(() => {
  document.removeEventListener('click', handleClickOutside);
});

const formatDate = (iso?: string | null) => {
  if (!iso) return 'N/A';
  return new Date(iso).toLocaleDateString(undefined, {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
  });
};
</script>

<template>
  <header class="sticky top-0 z-40 w-full backdrop-blur-md bg-white/95 border-b border-slate-200/80 shadow-2xs">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 h-16 flex items-center justify-between gap-4">
      <div class="flex items-center">
        <span class="text-xl font-bold text-slate-900 tracking-tight">
          Notes Application
        </span>
      </div>

      <div class="relative" ref="dropdownRef">
        <button
          @click.stop="toggleDropdown"
          class="flex items-center gap-2 hover:bg-slate-100/60 py-1.5 px-3 rounded-full transition-colors cursor-pointer text-left focus:outline-none"
        >
            <div class="w-9 h-9 rounded-full bg-blue-50 border border-blue-200 flex items-center justify-center text-blue-700 font-bold text-xs shrink-0">
              {{ authStore.user?.username?.charAt(0).toUpperCase() || 'U' }}
            </div>
            <div class="hidden sm:block text-left">
              <p class="text-[14px] font-semibold text-slate-800 leading-tight">
                {{ authStore.user?.username || 'User' }}
              </p>
              <p class="text-[14px] text-slate-500 truncate max-w-[120px]">
                {{ authStore.user?.email }}
              </p>
            </div>
            <ChevronDown
              class="w-3.5 h-3.5 text-slate-400 transition-transform duration-150"
              :class="{ 'rotate-180': isDropdownOpen }"
            />
          </button>

          <div
            v-if="isDropdownOpen"
            class="absolute right-0 mt-2 w-56 bg-white border border-slate-200/90 rounded-xl shadow-xl p-1.5 z-50 animate-in fade-in duration-100"
          >
            <div class="px-3 py-2 border-b border-slate-100">
              <p class="text-[14px] font-semibold text-slate-900 leading-tight">
                {{ authStore.user?.username }}
              </p>
              <p class="text-[14px] text-slate-500 truncate mt-0.5">
                {{ authStore.user?.email }}
              </p>
            </div>

            <div class="pt-1 space-y-0.5">
              <button
                @click="openProfile"
                class="w-full flex items-center gap-2.5 px-3 py-2 text-[14px] font-medium text-slate-700 hover:bg-slate-50 rounded-lg transition-colors cursor-pointer text-left"
              >
                <User class="w-4 h-4 text-blue-600" />
                <span>My Profile</span>
              </button>

              <button
                @click="handleLogout"
                class="w-full flex items-center gap-2.5 px-3 py-2 text-[14px] font-medium text-red-600 hover:bg-red-50 rounded-lg transition-colors cursor-pointer text-left"
              >
                <LogOut class="w-4 h-4" />
                <span>Log Out</span>
              </button>
            </div>
          </div>
        </div>
      </div>
  </header>

  <Transition name="modal-fade">
    <div v-if="isProfileModalOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4">
      <div
        class="fixed inset-0 bg-black/50"
        @click="isProfileModalOpen = false"
      ></div>

      <div
        class="modal-content relative z-10 grid w-full max-w-md gap-4 border border-slate-200 bg-white p-6 shadow-2xl rounded-2xl"
      >
      <div class="flex items-center justify-between pb-3 border-b border-slate-100">
        <h3 class="text-lg font-bold text-slate-900">
          {{ isEditingProfile ? 'Change Information' : 'User Profile' }}
        </h3>
        <button
          @click="isProfileModalOpen = false"
          class="p-1 rounded-full text-slate-400 hover:text-slate-700 hover:bg-slate-100 transition-colors cursor-pointer"
        >
          <X class="w-4 h-4" />
        </button>
      </div>

      <div v-if="!isEditingProfile" class="py-2 space-y-3.5">
        <div class="flex items-center gap-3">
          <div class="w-12 h-12 rounded-full bg-blue-50 border border-blue-200 text-blue-700 font-bold text-base flex items-center justify-center">
            {{ authStore.user?.username?.charAt(0).toUpperCase() || 'U' }}
          </div>
          <div class="text-left">
            <p class="text-base font-bold text-slate-900">{{ authStore.user?.username }}</p>
            <p class="text-sm text-slate-500">Active Member</p>
          </div>
        </div>

        <div class="space-y-2 pt-2 text-sm border-t border-slate-100 text-left">
          <div class="flex items-center gap-2 text-slate-600">
            <Mail class="w-4 h-4 text-blue-600" />
            <span class="text-slate-800 font-medium">{{ authStore.user?.email }}</span>
          </div>
          <div class="flex items-center gap-2 text-slate-600">
            <Calendar class="w-4 h-4 text-slate-400" />
            <span class="text-slate-500">Member since: {{ formatDate(authStore.user?.createdAt) }}</span>
          </div>
        </div>
      </div>

      <div v-else class="py-1 space-y-3.5">
        <div v-if="profileError" class="p-3 bg-red-50 border border-red-200 rounded-xl text-sm text-red-600 font-medium text-left">
          {{ profileError }}
        </div>
        <div v-if="profileSuccess" class="p-3 bg-green-50 border border-green-200 rounded-xl text-sm text-green-700 font-medium flex items-center gap-2">
          <Check class="w-4 h-4 text-green-600 shrink-0" />
          <span>{{ profileSuccess }}</span>
        </div>

        <div class="space-y-1.5 text-left">
          <label class="text-sm font-semibold text-slate-700 leading-none">
            Username <span class="text-red-500">*</span>
          </label>
          <input
            v-model="editUsername"
            type="text"
            placeholder="Enter new username"
            class="flex h-11 w-full rounded-xl border border-slate-200 bg-slate-50/60 px-3.5 py-1 text-base text-slate-900 placeholder:text-slate-400 focus:bg-white focus:outline-none focus:border-blue-600 focus:ring-1 focus:ring-blue-600 transition-colors"
          />
        </div>

        <div class="space-y-1.5 text-left">
          <label class="text-sm font-semibold text-slate-700 leading-none">
            New Password <span class="text-xs text-slate-400 font-normal">(Optional)</span>
          </label>
          <input
            v-model="editPassword"
            type="password"
            placeholder="Leave blank to keep current password"
            class="flex h-11 w-full rounded-xl border border-slate-200 bg-slate-50/60 px-3.5 py-1 text-base text-slate-900 placeholder:text-slate-400 focus:bg-white focus:outline-none focus:border-blue-600 focus:ring-1 focus:ring-blue-600 transition-colors"
          />
        </div>

        <div class="space-y-1.5 text-left">
          <label class="text-sm font-semibold text-slate-700 leading-none">
            Confirm Password <span class="text-xs text-slate-400 font-normal">(Optional)</span>
          </label>
          <input
            v-model="editConfirmPassword"
            type="password"
            placeholder="Confirm new password"
            class="flex h-11 w-full rounded-xl border border-slate-200 bg-slate-50/60 px-3.5 py-1 text-base text-slate-900 placeholder:text-slate-400 focus:bg-white focus:outline-none focus:border-blue-600 focus:ring-1 focus:ring-blue-600 transition-colors"
          />
        </div>
      </div>

      <div class="pt-3 border-t border-slate-100 flex items-center justify-end gap-2.5">
        <template v-if="!isEditingProfile">
          <button
            @click="isProfileModalOpen = false"
            class="px-4.5 py-2 text-sm font-medium text-slate-700 bg-slate-100 hover:bg-slate-200 rounded-full transition-colors cursor-pointer"
          >
            Close
          </button>
          <button
            @click="startEditProfile"
            class="inline-flex items-center gap-1.5 px-4.5 py-2 text-sm font-medium text-white bg-[#0f3a5f] hover:bg-[#0a2540] rounded-full transition-all active:scale-95 cursor-pointer shadow-sm"
          >
            <Edit3 class="w-4 h-4" />
            <span>Change Information</span>
          </button>
        </template>
        <template v-else>
          <button
            @click="cancelEditProfile"
            class="px-4.5 py-2 text-sm font-medium text-slate-700 bg-slate-100 hover:bg-slate-200 rounded-full transition-colors cursor-pointer"
          >
            Cancel
          </button>
          <button
            @click="handleSaveProfile"
            :disabled="authStore.loading"
            class="inline-flex items-center gap-1.5 px-5 py-2 text-sm font-medium text-white bg-[#0f3a5f] hover:bg-[#0a2540] rounded-full transition-all active:scale-95 cursor-pointer shadow-sm disabled:opacity-50"
          >
            <Check class="w-4 h-4" />
            <span>{{ authStore.loading ? 'Saving...' : 'Save Changes' }}</span>
          </button>
        </template>
      </div>
    </div>
  </div>
  </Transition>
</template>
