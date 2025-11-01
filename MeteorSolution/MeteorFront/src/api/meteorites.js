import axios from "axios";

const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || "http://localhost:5000", // для дебага
  timeout: 5000,
});

export async function getMeteorites(filters) {
  try {
    const response = await api.get("/api/meteorites/filter", { 
      params: filters, 
      paramsSerializer: (params) => {
        return Object.keys(params)
          .filter(key => params[key] !== undefined && params[key] !== null)
          .map(key => `${key}=${encodeURIComponent(params[key])}`)
          .join('&');
      }
    });

    return response.data;
  } catch (error) {
    throw new Error("Не могу загрузить данные — проверь консоль!");
  }
}