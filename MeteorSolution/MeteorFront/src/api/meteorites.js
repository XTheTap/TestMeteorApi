const apiBase = import.meta.env.VITE_API_BASE_URL || "http://localhost:5000";

function buildQuery(filters) {
  if (!filters) return "";
  const parts = Object.keys(filters)
    .filter(k => filters[k] !== undefined && filters[k] !== null && filters[k] !== "")
    .map(k => `${encodeURIComponent(k)}=${encodeURIComponent(filters[k])}`);
  return parts.length ? `?${parts.join("&")}` : "";
}

function normalizeItem(obj) {
  return {
    meteorName: obj.meteorName ?? obj.MeteorName ?? obj.name ?? "Unknown",
    recclassText: obj.recclassText ?? obj.RecclassText ?? obj.recclass ?? "Unknown",
    year: obj.year ?? obj.Year ?? null,
    count: obj.count ?? obj.Count ?? 0,
    totalMass: obj.totalMass ?? obj.TotalMass ?? 0,
    ...obj,
  };
}

export async function getMeteorites(filters) {
  const url = `${apiBase}/api/meteorites/filter${buildQuery(filters)}`;

  const res = await fetch(url, { method: "GET" });
  if (!res.ok) throw new Error(`Request failed: ${res.status}`);

  const results = [];
  const reader = res.body.getReader();
  const decoder = new TextDecoder();
  let buffer = "";

  while (true) {
    const { value, done } = await reader.read();
    if (done) break;
    buffer += value ? decoder.decode(value, { stream: true }) : "";

    const lines = buffer.split('\n');
    buffer = lines.pop();

    for (const line of lines) {
      if (!line) continue;
      try {
        const parsed = JSON.parse(line);
        results.push(normalizeItem(parsed));
      } catch (e) {
        console.error('ndjson parse error', e);
      }
    }
  }

  if (buffer) {
    try {
      const parsed = JSON.parse(buffer);
      results.push(normalizeItem(parsed));
    } catch (e) {
      console.error('ndjson final parse error', e);
    }
  }

  return results;
}

export async function getMeteoriteTypes() {
  const url = `${apiBase}/api/meteorites/types`;
  const res = await fetch(url, { method: "GET" });
  if (!res.ok) throw new Error(`Request failed: ${res.status}`);
  return await res.json();
}