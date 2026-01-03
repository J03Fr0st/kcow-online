/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{html,ts}'],
  theme: {
    extend: {
      colors: {
        primary: '#3b82f6',
        secondary: '#8b5cf6',
        accent: '#f59e0b',
        neutral: '#1f2937',
      },
    },
  },
  plugins: [require('daisyui')],
  daisyui: {
    themes: ['dark'], // Only dark theme enabled as per Story 1.5
    darkTheme: 'dark',
    base: true,
    styled: true,
    utils: true,
    logs: false,
  },
};
