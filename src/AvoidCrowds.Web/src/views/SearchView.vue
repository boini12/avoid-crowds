<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { getHealth, type StationSuggestion } from '../lib/apiClient'
import { journeySearchParams } from '../lib/journeySearchStore'
import { toIsoStringWithLocalOffset } from '../lib/dateTime'
import StationAutocomplete from '../components/StationAutocomplete.vue'

type ApiStatus = 'checking' | 'reachable' | 'unreachable'

const router = useRouter()

const apiStatus = ref<ApiStatus>('checking')
const fromStation = ref<StationSuggestion | null>(null)
const toStation = ref<StationSuggestion | null>(null)
const date = ref('')
const time = ref('')
const arriveBy = ref(false)

const isSameStation = computed(
  () => fromStation.value !== null && toStation.value !== null && fromStation.value.id === toStation.value.id,
)

const canSubmit = computed(
  () => fromStation.value !== null && toStation.value !== null && date.value !== '' && time.value !== '' && !isSameStation.value,
)

onMounted(async () => {
  try {
    await getHealth()
    apiStatus.value = 'reachable'
  } catch {
    apiStatus.value = 'unreachable'
  }
})

function onSubmit() {
  if (!canSubmit.value || fromStation.value === null || toStation.value === null) {
    return
  }

  journeySearchParams.value = {
    fromStation: fromStation.value,
    toStation: toStation.value,
    time: toIsoStringWithLocalOffset(date.value, time.value),
    arriveBy: arriveBy.value,
  }

  void router.push('/results')
}
</script>

<template>
  <main class="mx-auto flex min-h-screen max-w-xl flex-col items-center justify-center gap-4 px-4 py-8">
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
    <form class="flex w-full flex-col gap-4" @submit.prevent="onSubmit">
      <StationAutocomplete v-model="fromStation" label="From" />
      <StationAutocomplete v-model="toStation" label="To" />
      <p v-if="isSameStation" class="text-sm text-red-700">Origin and destination must be different stations.</p>
      <div class="flex gap-4">
        <div class="flex flex-1 flex-col gap-1">
          <label for="search-date" class="text-sm font-medium text-gray-700">Date</label>
          <input
            id="search-date"
            v-model="date"
            type="date"
            class="rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none"
          />
        </div>
        <div class="flex flex-1 flex-col gap-1">
          <label for="search-time" class="text-sm font-medium text-gray-700">Time</label>
          <input
            id="search-time"
            v-model="time"
            type="time"
            class="rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none"
          />
        </div>
      </div>
      <fieldset class="flex gap-4">
        <legend class="mb-1 text-sm font-medium text-gray-700">Search by</legend>
        <label class="flex items-center gap-2 text-sm text-gray-700">
          <input v-model="arriveBy" type="radio" name="search-direction" :value="false" />
          Depart after
        </label>
        <label class="flex items-center gap-2 text-sm text-gray-700">
          <input v-model="arriveBy" type="radio" name="search-direction" :value="true" />
          Arrive by
        </label>
      </fieldset>
      <button
        type="submit"
        class="rounded-md bg-blue-600 px-4 py-2 font-medium text-white disabled:cursor-not-allowed disabled:bg-gray-300"
        :disabled="!canSubmit"
      >
        Search
      </button>
    </form>
  </main>
</template>
