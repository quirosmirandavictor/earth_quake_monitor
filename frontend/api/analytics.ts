import type { VercelRequest, VercelResponse } from "@vercel/node";

export default async function handler(req: VercelRequest, res: VercelResponse) {
  const base = process.env.EARTHQUAKE_API_URL, functionKey = process.env.EARTHQUAKE_FUNCTION_KEY, apiKey = process.env.EARTHQUAKE_API_KEY;
  if (!base || !functionKey || !apiKey) return res.status(503).json({ error: "proxy_not_configured" });
  const upstream = `${base.replace(/\/$/, "")}/api/v1/analytics/earthquakes?${new URLSearchParams(req.query as Record<string, string>).toString()}`;
  const response = await fetch(upstream, { headers: { "x-functions-key": functionKey, "x-api-key": apiKey } });
  res.status(response.status).setHeader("Cache-Control", "s-maxage=60, stale-while-revalidate=300");
  return res.send(await response.text());
}
