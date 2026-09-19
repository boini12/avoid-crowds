import { ref } from 'vue'
import type { StationSuggestion } from './apiClient'

export interface JourneySearchParams {
  fromStation: StationSuggestion
  toStation: StationSuggestion
  time: string
  arriveBy: boolean
}

export const journeySearchParams = ref<JourneySearchParams | null>(null)
