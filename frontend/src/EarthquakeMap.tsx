import { MapContainer, CircleMarker, Popup, TileLayer, useMap } from "react-leaflet";
import { useEffect } from "react";
import type { Earthquake } from "./api";
import "leaflet/dist/leaflet.css";

const formatOriginTime = (value: string) =>
  new Intl.DateTimeFormat("en-US", {
    dateStyle: "medium",
    timeStyle: "short",
    timeZone: "UTC"
  }).format(new Date(value));

function MapViewport({ events }: { events: Earthquake[] }) {
  const map = useMap();
  useEffect(() => {
    if (events.length === 0) return;
    map.fitBounds(events.map(event => [event.latitude, event.longitude] as [number, number]), { padding: [24, 24], maxZoom: 7 });
  }, [events, map]);
  return null;
}

export function EarthquakeMap({ events }: { events: Earthquake[] }) {
  return <section className="map-card">
    <div className="map-heading"><div><p className="eyebrow">EVENT MAP</p><h2>Recent seismic activity</h2></div><span>{events.length} events</span></div>
    <MapContainer className="event-map" center={[9.93, -84.08]} zoom={5} scrollWheelZoom>
      <TileLayer attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors' url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png" />
      <MapViewport events={events} />
      {events.map(event => <CircleMarker key={`${event.source}-${event.externalId}`} center={[event.latitude, event.longitude]} radius={Math.max(6, Math.min(16, 6 + (event.magnitude ?? 0) * 1.5))} pathOptions={{ color: "#a78bfa", fillColor: "#6d4bd2", fillOpacity: .82, weight: 2 }}>
        <Popup><strong>Magnitude {event.magnitude?.toFixed(1) ?? "-"}</strong><br />{event.place ?? "Location unavailable"}<br />{formatOriginTime(event.originTime)} UTC<br />{event.latitude.toFixed(3)}, {event.longitude.toFixed(3)}</Popup>
      </CircleMarker>)}
    </MapContainer>
  </section>;
}
