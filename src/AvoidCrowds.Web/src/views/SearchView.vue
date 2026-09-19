<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { getHealth } from '../lib/apiClient'

type ApiStatus = 'checking' | 'reachable' | 'unreachable'

const apiStatus = ref<ApiStatus>('checking')

onMounted(async () => {
  try {
    await getHealth()
    apiStatus.value = 'reachable'
  } catch {
    apiStatus.value = 'unreachable'
  }
})
</script>

<template>
  <main class="mx-auto flex min-h-screen max-w-xl flex-col items-center justify-center gap-4 px-4">
    <h1 class="text-2xl font-medium text-gray-900">Avoid Crowds</h1>
    <p class="text-gray-600">Plan a train journey and check whether it'll be crowded with football fans.</p>
    <p
      class="rounded-full px-3 py-1 text-sm"
      :class="{
        'bg-gray-100 text-gray-600': apiStatus === 'checking',
        'bg-green-100 text-green-800': apiStatus === 'reachable',
        'bg-red-100 text-red-800': apiStatus === 'unreachable',
      }"
    >
      <span v-if="apiStatus === 'checking'">Checking API…</span>
      <span v-else-if="apiStatus === 'reachable'">API reachable</span>
      <span v-else>API unreachable</span>
    </p>
  </main>
</template>
