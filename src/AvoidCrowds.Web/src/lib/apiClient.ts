const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5027'

export interface HealthResponse {
  status: string
}

export async function getHealth(): Promise<HealthResponse> {
  const response = await fetch(`${API_BASE_URL}/api/health`)

  if (!response.ok) {
    throw new Error(`Health check failed with status ${response.status}`)
  }

  return response.json()
}

export interface StationSuggestion {
  id: string
  name: string
  lat: number
  lon: number
}

export async function getStations(query: string, signal?: AbortSignal): Promise<StationSuggestion[]> {
  const response = await fetch(`${API_BASE_URL}/api/stations?query=${encodeURIComponent(query)}`, { signal })

  if (!response.ok) {
    throw new Error(`Station search failed with status ${response.status}`)
  }

  return response.json()
}

export interface DirectTrain {
  origin: string
  destination: string
  departureTime: string
  tripId: string
}

export interface JourneySearchRequest {
  fromStationId: string
  toStationId: string
  time: string
  arriveBy: boolean
}

export async function getJourneys(request: JourneySearchRequest, signal?: AbortSignal): Promise<DirectTrain[]> {
  const params = new URLSearchParams({
    fromStationId: request.fromStationId,
    toStationId: request.toStationId,
    time: request.time,
    arriveBy: String(request.arriveBy),
  })

  const response = await fetch(`${API_BASE_URL}/api/journeys?${params.toString()}`, { signal })

  if (!response.ok) {
    throw new Error(`Journey search failed with status ${response.status}`)
  }

  return response.json()
}

export interface TrainStop {
  name: string
  arrival: string | null
  departure: string | null
  timeZone: string
}

export interface CrowdWarning {
  stopName: string
  homeTeam: string
  awayTeam: string
  kickoffTime: string
  timeZone: string
}

export interface TrainDetail {
  origin: string
  destination: string
  departureTime: string
  departureTimeZone: string
  arrivalTime: string
  arrivalTimeZone: string
  stops: TrainStop[]
  crowdWarnings: CrowdWarning[]
}

export async function getTrainDetail(tripId: string, signal?: AbortSignal): Promise<TrainDetail> {
  const params = new URLSearchParams({ tripId })
  const response = await fetch(`${API_BASE_URL}/api/journeys/trip?${params.toString()}`, { signal })

  if (!response.ok) {
    throw new Error(`Train detail lookup failed with status ${response.status}`)
  }

  return response.json()
}
