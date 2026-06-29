import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import tailwindcss from "@tailwindcss/vite";

// https://vite.dev/config/
import { VitePWA } from "vite-plugin-pwa";

export default defineConfig({
  base: "/chatapp/",
  plugins: [
    react(),
    tailwindcss(),
    VitePWA({
      registerType: "autoUpdate",
      includeAssets: ["favicon.svg", "robots.txt", "apple-touch-icon.png"],
      manifest: {
        name: "SabrinaChat",
        short_name: "Chat",
        description: "Encrypted chat app",
        theme_color: "#ff5ca2",
        background_color: "#fff0f6",
        display: "standalone",
        scope: "/chatapp/",
        start_url: "/chatapp/",
        icons: [
          {
            src: "pwa-192x192.png",
            sizes: "192x192",
            type: "image/png",
          },
          {
            src: "pwa-512x512.png",
            sizes: "512x512",
            type: "image/png",
          },
        ],
      },
    }),
  ],
});
