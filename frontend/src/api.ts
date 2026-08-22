export type Earthquake = { id: string; source: string; externalId: string; originTime: string; latitude: number; longitude: number; depthKm?: number | null; magnitude?: number | null; place?: string | null };
export type SearchResponse = { items: Earthquake[]; count: number; limit: number };

export async function searchEarthquakes(params: URLSearchParams): Promise<SearchResponse> {
  const response = await fetch(`/api/earthquakes?${params}`);
  if (!response.ok) throw new Error(response.status === 429 ? "Too many requests. Try again in a moment." : "The events could not be queried.");
  return response.json();
}
