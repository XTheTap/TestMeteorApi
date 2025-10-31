import axios from "axios";

const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  timeout: 5000,
});

export async function getMeteorites(filters) {
  try {
    const response = await api.get("/api/meteorites/filter", { params: filters });
    return response.data;
  } catch (error) {
    console.error("API error:", error);
    throw new Error("Can't load servers data");
  }
}