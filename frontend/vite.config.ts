import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

const backendUrl = process.env.EARTHQUAKE_API_URL ?? "http://localhost:7071";

export default defineConfig({
  plugins: [react()],
  server: {
    host: "0.0.0.0",
    port: 5173,
    proxy: {
      "/api/earthquakes": {
        target: backendUrl,
        changeOrigin: true,
        rewrite: path => path.replace(/^\/api\/earthquakes/, "/api/v1/earthquakes"),
        headers: {
          "x-functions-key": process.env.EARTHQUAKE_FUNCTION_KEY ?? "",
          "x-api-key": process.env.EARTHQUAKE_API_KEY ?? ""
        }
      },
      "/api/analytics": {
        target: backendUrl,
        changeOrigin: true,
        rewrite: path => path.replace(/^\/api\/analytics/, "/api/v1/analytics/earthquakes"),
        headers: {
          "x-functions-key": process.env.EARTHQUAKE_FUNCTION_KEY ?? "",
          "x-api-key": process.env.EARTHQUAKE_API_KEY ?? ""
        }
      }
    }
  }
});
