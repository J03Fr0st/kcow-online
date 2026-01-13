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
  esbuild: {
    target: 'es2023',
    legalComments: 'none',
    minifyIdentifiers: false,
    minifySyntax: false,
    minifyWhitespace: false,
  },
  build: {
    target: 'es2023',
    outDir: 'dist',
    emptyOutDir: true,
    minify: 'esbuild',
    sourcemap: true,
    rollupOptions: {
      output: {
        manualChunks: {
          'vendor': ['@angular/core', '@angular/common', '@angular/router'],
        },
      },
    },
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
    esbuildOptions: {
      target: 'es2023',
    },
  },
});
