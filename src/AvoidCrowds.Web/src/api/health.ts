const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5027'

export interface HealthResponse {
  status: string
}

export async function checkHealth(): Promise<HealthResponse> {
  const response = await fetch(`${API_BASE_URL}/api/health`)

  if (!response.ok) {
    throw new Error(`Health check failed with status ${response.status}`)
  }

  return (await response.json()) as HealthResponse
}
