import { defineConfig, devices } from '@playwright/test';

/**
 * Playwright E2E Testing Configuration
 *
 * IMPORTANT: This config requires BOTH frontend and backend servers to be running.
 * Use the provided scripts to start both servers before running tests:
 * - Windows: e2e-test.bat
 * - Unix/Mac: e2e-test.sh (make executable first)
 *
 * Or manually:
 * 1. Start backend: cd apps/backend/src/Api && dotnet run
 * 2. Start frontend: cd apps/frontend && npm run dev
 * 3. Run tests: npm run test:e2e
 */
export default defineConfig({
  testDir: './e2e',
  testMatch: '**/*.spec.ts', // Only match spec files in testDir
  testIgnore: '**/node_modules/**', // Explicitly ignore node_modules
  fullyParallel: false, // Run tests serially to avoid state sharing issues
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: 1, // Single worker to prevent parallel test interference
  reporter: 'html',

  use: {
    baseURL: 'http://localhost:4200',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',
    // Environment variables for test credentials
    extraHTTPHeaders: {
      'X-Test-Email': process.env.TEST_EMAIL || 'admin@kcow.local',
      'X-Test-Password': process.env.TEST_PASSWORD || 'Admin123!',
    },
  },

  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
    {
      name: 'firefox',
      use: { ...devices['Desktop Firefox'] },
    },
    {
      name: 'webkit',
      use: { ...devices['Desktop Safari'] },
    },
  ],

  // Expect servers to already be running
  // Use e2e-test.bat or e2e-test.sh to run tests with server startup
});
