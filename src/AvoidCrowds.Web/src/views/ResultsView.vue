<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { getJourneys, type DirectTrain } from '../lib/apiClient'
import { journeySearchParams } from '../lib/journeySearchStore'
import { selectedTripId } from '../lib/selectedTrainStore'
import ErrorBanner from '../components/ErrorBanner.vue'

const router = useRouter()

const trains = ref<DirectTrain[]>([])
const isLoading = ref(false)
const error = ref<string | null>(null)

onMounted(() => {
  if (journeySearchParams.value === null) {
    void router.replace('/')
    return
  }

  void search()
})

async function search() {
  const params = journeySearchParams.value
  if (params === null) {
    return
  }

  isLoading.value = true
  error.value = null

  try {
    trains.value = await getJourneys({
      fromStationId: params.fromStation.id,
      toStationId: params.toStation.id,
      time: params.time,
      arriveBy: params.arriveBy,
    })
  } catch {
    error.value = 'Could not load train results.'
  } finally {
    isLoading.value = false
  }
}

function formatDepartureTime(departureTime: string) {
  return new Date(departureTime).toLocaleString(undefined, { dateStyle: 'medium', timeStyle: 'short' })
}

function selectTrain(train: DirectTrain) {
  selectedTripId.value = train.tripId
  void router.push('/train')
}

function backToStart() {
  void router.push('/')
}
</script>

<template>
  <main class="mx-auto flex min-h-screen max-w-xl flex-col gap-4 px-4 py-8">
    <h1 class="text-2xl font-medium text-gray-900">Direct trains</h1>

    <p v-if="isLoading" class="text-gray-600">Searching for trains…</p>

    <ErrorBanner v-else-if="error" :message="error" @retry="search" />

    <template v-else>
      <p v-if="trains.length === 0" class="text-gray-600">No direct trains found.</p>
      <ul v-else class="flex flex-col gap-3">
        <li v-for="train in trains" :key="train.tripId">
          <button
            type="button"
            class="w-full rounded-md border border-gray-200 px-4 py-3 text-left hover:bg-gray-50"
            @click="selectTrain(train)"
          >
            <p class="font-medium text-gray-900">{{ train.origin }} → {{ train.destination }}</p>
            <p class="text-sm text-gray-600">Departs {{ formatDepartureTime(train.departureTime) }}</p>
          </button>
        </li>
      </ul>
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
