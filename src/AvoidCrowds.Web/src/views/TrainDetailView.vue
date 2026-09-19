<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { getTrainDetail, type TrainDetail } from '../lib/apiClient'
import { selectedTripId } from '../lib/selectedTrainStore'
import ErrorBanner from '../components/ErrorBanner.vue'

const router = useRouter()

const detail = ref<TrainDetail | null>(null)
const isLoading = ref(false)
const error = ref<string | null>(null)

onMounted(() => {
  if (selectedTripId.value === null) {
    void router.replace('/')
    return
  }

  void load()
})

async function load() {
  const tripId = selectedTripId.value
  if (tripId === null) {
    return
  }

  isLoading.value = true
  error.value = null

  try {
    detail.value = await getTrainDetail(tripId)
  } catch {
    error.value = 'Could not load train details.'
  } finally {
    isLoading.value = false
  }
}

// Explicit `timeZone` avoids formatting in the browser's own timezone,
// which would be wrong for a stop whose local time differs from the viewer's.
function formatStopTime(time: string | null, timeZone: string) {
  if (time === null) {
    return null
  }

  return new Date(time).toLocaleString(undefined, { dateStyle: 'medium', timeStyle: 'short', timeZone })
}

function backToStart() {
  void router.push('/')
}
</script>

<template>
  <main class="mx-auto flex min-h-screen max-w-xl flex-col gap-4 px-4 py-8">
    <h1 class="text-2xl font-medium text-gray-900">Train details</h1>

    <p v-if="isLoading" class="text-gray-600">Loading train details…</p>

    <ErrorBanner v-else-if="error" :message="error" @retry="load" />

    <template v-else-if="detail">
      <div>
        <p class="font-medium text-gray-900">{{ detail.origin }} → {{ detail.destination }}</p>
        <p class="text-sm text-gray-600">
          Departs {{ formatStopTime(detail.departureTime, detail.departureTimeZone) }} · Arrives
          {{ formatStopTime(detail.arrivalTime, detail.arrivalTimeZone) }}
        </p>
      </div>

      <ol class="flex flex-col gap-3">
        <li
          v-for="(stop, index) in detail.stops"
          :key="index"
          class="rounded-md border border-gray-200 px-4 py-3"
        >
          <p class="font-medium text-gray-900">{{ stop.name }}</p>
          <p class="text-sm text-gray-600">
            <span v-if="stop.arrival">Arrives {{ formatStopTime(stop.arrival, stop.timeZone) }}</span>
            <span v-if="stop.arrival && stop.departure"> · </span>
            <span v-if="stop.departure">Departs {{ formatStopTime(stop.departure, stop.timeZone) }}</span>
          </p>
        </li>
      </ol>
    </template>

    <button
      type="button"
      class="self-start rounded-md border border-gray-300 px-4 py-2 font-medium text-gray-700 hover:bg-gray-50"
      @click="backToStart"
    >
      Back to start
    </button>
  </main>
</template>
