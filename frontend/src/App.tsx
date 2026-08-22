import { FormEvent, useEffect, useState } from "react";
import { Earthquake, searchEarthquakes } from "./api";
import { EarthquakeMap } from "./EarthquakeMap";

export function App() {
  const [region, setRegion] = useState("Global");
  const [minimumMagnitude, setMinimumMagnitude] = useState(0);
  const [events, setEvents] = useState<Earthquake[]>([]);
  const [error, setError] = useState("");

  const load = async (event?: FormEvent) => {
    event?.preventDefault();
    setError("");
    try {
      const params = new URLSearchParams({ region, minMagnitude: String(minimumMagnitude), limit: "100" });
      setEvents((await searchEarthquakes(params)).items);
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : "Unexpected error");
    }
  };

  useEffect(() => { void load(); }, []);

  return <main>
    <header>
      <p className="eyebrow">EARTHQUAKE MONITOR</p>
      <h1>Seismic activity, without the noise.</h1>
      <p className="lead">Recent events queried through a protected, regionalized API.</p>
      <p className="history-note">The map and event list show stored events from the last 7 days. Use the filters to narrow the visible results.</p>
    </header>
    <form onSubmit={load} className="filters">
      <label>Region<select value={region} onChange={event => setRegion(event.target.value)}><option>Global</option><option>CostaRica</option><option>CentralAmerica</option><option>Caribbean</option></select></label>
      <label>Minimum magnitude<input type="number" min="0" max="10" step="0.1" value={minimumMagnitude} onChange={event => setMinimumMagnitude(Number(event.target.value))} /></label>
      <button>Refresh</button>
    </form>
    {error && <p className="error">{error}</p>}
    <EarthquakeMap events={events} />
    <section className="grid">{events.map(event => <article key={`${event.source}-${event.externalId}`}>
      <div className="magnitude">{event.magnitude?.toFixed(1) ?? "-"}</div>
      <div><time>{new Date(event.originTime).toLocaleString()}</time><h2>{event.place ?? "Location unavailable"}</h2><p>{event.latitude?.toFixed(3) ?? "-"}, {event.longitude?.toFixed(3) ?? "-"} · {event.depthKm?.toFixed(1) ?? "-"} km</p></div>
    </article>)}</section>
  </main>;
}
