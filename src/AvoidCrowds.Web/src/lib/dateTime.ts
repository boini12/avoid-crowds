// Builds an ISO 8601 string that keeps the browser's own UTC offset (rather than
// normalizing to UTC via Date#toISOString) so the backend can tell which calendar
// day the traveler meant — it uses the offset to bound the anchor-day search window.
export function toIsoStringWithLocalOffset(date: string, time: string): string {
  const local = new Date(`${date}T${time}:00`)
  const offsetMinutes = -local.getTimezoneOffset()
  const sign = offsetMinutes >= 0 ? '+' : '-'
  const pad = (value: number) => String(value).padStart(2, '0')

  const offsetHours = pad(Math.floor(Math.abs(offsetMinutes) / 60))
  const offsetMins = pad(Math.abs(offsetMinutes) % 60)

  return `${date}T${time}:00${sign}${offsetHours}:${offsetMins}`
}
