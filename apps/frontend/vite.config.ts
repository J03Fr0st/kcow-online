import { defineConfig } from 'vite';
import angular from '@analogjs/vite-plugin-angular';

export default defineConfig({
  plugins: [angular()],
  resolve: {
    mainFields: ['module'],
    alias: {
      '@core': '/src/app/core',
      '@shared': '/src/app/shared',
      '@features': '/src/app/features',
      '@layouts': '/src/app/layouts',
      '@models': '/src/app/models',
      '@environments': '/src/environments',
    },
  },
  build: {
    target: 'es2023',
    outDir: 'dist',
    emptyOutDir: true,
  },
  server: {
    port: 4200,
    open: true,
    fs: {
      strict: false,
    },
  },
  optimizeDeps: {
    include: ['@angular/common', '@angular/core', '@angular/forms', '@angular/router'],
  },
});
