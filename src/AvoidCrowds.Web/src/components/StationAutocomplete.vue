<script setup lang="ts">
import { ref, watch } from 'vue'
import { getStations, type StationSuggestion } from '../lib/apiClient'

const MIN_QUERY_LENGTH = 2
const DEBOUNCE_MS = 250

const props = defineProps<{
  label: string
  modelValue: StationSuggestion | null
}>()

const emit = defineEmits<{
  'update:modelValue': [station: StationSuggestion | null]
}>()

const inputId = `station-autocomplete-${Math.random().toString(36).slice(2)}`

const query = ref(props.modelValue?.name ?? '')
const suggestions = ref<StationSuggestion[]>([])
const isOpen = ref(false)
const isLoading = ref(false)
const highlightedIndex = ref(-1)

let debounceTimer: ReturnType<typeof setTimeout> | undefined
let abortController: AbortController | undefined

watch(
  () => props.modelValue,
  (station) => {
    query.value = station?.name ?? ''
  },
)

function onInput() {
  emit('update:modelValue', null)
  highlightedIndex.value = -1

  clearTimeout(debounceTimer)
  abortController?.abort()

  const trimmed = query.value.trim()
  if (trimmed.length < MIN_QUERY_LENGTH) {
    suggestions.value = []
    isOpen.value = false
    return
  }

  debounceTimer = setTimeout(() => void search(trimmed), DEBOUNCE_MS)
}

async function search(text: string) {
  abortController = new AbortController()
  isLoading.value = true

  try {
    suggestions.value = await getStations(text, abortController.signal)
    isOpen.value = true
  } catch (error) {
    if (error instanceof DOMException && error.name === 'AbortError') {
      return
    }
    suggestions.value = []
  } finally {
    isLoading.value = false
  }
}

function selectStation(station: StationSuggestion) {
  emit('update:modelValue', station)
  query.value = station.name
  suggestions.value = []
  isOpen.value = false
  highlightedIndex.value = -1
}

function onKeydown(event: KeyboardEvent) {
  if (!isOpen.value || suggestions.value.length === 0) {
    return
  }

  if (event.key === 'ArrowDown') {
    event.preventDefault()
    highlightedIndex.value = (highlightedIndex.value + 1) % suggestions.value.length
  } else if (event.key === 'ArrowUp') {
    event.preventDefault()
    highlightedIndex.value =
      (highlightedIndex.value - 1 + suggestions.value.length) % suggestions.value.length
  } else if (event.key === 'Enter' && highlightedIndex.value >= 0) {
    event.preventDefault()
    selectStation(suggestions.value[highlightedIndex.value]!)
  } else if (event.key === 'Escape') {
    isOpen.value = false
  }
}

function onBlur() {
  // Delay so a click on a suggestion registers before the list closes.
  setTimeout(() => {
    isOpen.value = false
  }, 150)
}
</script>

<template>
  <div class="relative flex flex-col gap-1">
    <label :for="inputId" class="text-sm font-medium text-gray-700">{{ props.label }}</label>
    <input
      :id="inputId"
      v-model="query"
      type="text"
      role="combobox"
      aria-autocomplete="list"
      :aria-expanded="isOpen"
      class="rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none"
      @input="onInput"
      @keydown="onKeydown"
      @focus="() => (isOpen = suggestions.length > 0)"
      @blur="onBlur"
    />
    <ul
      v-if="isOpen && suggestions.length > 0"
      class="absolute top-full z-10 mt-1 max-h-60 w-full overflow-auto rounded-md border border-gray-200 bg-white shadow-lg"
    >
      <li
        v-for="(station, index) in suggestions"
        :key="station.id"
        class="cursor-pointer px-3 py-2"
        :class="{ 'bg-blue-100': index === highlightedIndex }"
        @mousedown.prevent="selectStation(station)"
      >
        {{ station.name }}
      </li>
    </ul>
    <p v-else-if="isLoading" class="absolute top-full mt-1 text-sm text-gray-500">Searching…</p>
  </div>
</template>
