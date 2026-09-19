<script setup lang="ts">
defineProps<{
  message: string
}>()

const emit = defineEmits<{
  retry: []
}>()
</script>

<template>
  <div
    role="alert"
    class="flex items-center justify-between gap-4 rounded-md border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-800"
  >
    <span>{{ message }}</span>
    <button
      type="button"
      class="shrink-0 rounded-md border border-red-300 bg-white px-3 py-1 font-medium text-red-700 hover:bg-red-100"
      @click="emit('retry')"
    >
      Retry
    </button>
  </div>
</template>
