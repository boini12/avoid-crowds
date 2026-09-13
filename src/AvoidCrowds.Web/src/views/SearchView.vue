<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { checkHealth } from '../api/health'

type ApiStatus = 'checking' | 'reachable' | 'unreachable'

const apiStatus = ref<ApiStatus>('checking')

onMounted(async () => {
  try {
    const health = await checkHealth()
    apiStatus.value = health.status === 'ok' ? 'reachable' : 'unreachable'
  } catch {
    apiStatus.value = 'unreachable'
  }
})
</script>

<template>
  <section>
    <h1>Search</h1>
    <p data-testid="api-status">
      <template v-if="apiStatus === 'checking'">Checking API connection…</template>
      <template v-else-if="apiStatus === 'reachable'">✅ API is reachable</template>
      <template v-else>❌ API is unreachable</template>
    </p>
  </section>
</template>
