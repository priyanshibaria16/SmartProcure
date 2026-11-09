/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        brand: {
          50: '#f0f5fa',
          100: '#e1ecf5',
          200: '#c3d9eb',
          300: '#95bde0',
          400: '#5f9cd0',
          500: '#387ebc',
          600: '#27649f',
          700: '#205080',
          800: '#1d446a',
          900: '#1c3957',
          950: '#0e1f31',
        },
        navy: {
          800: '#1e293b',
          900: '#0f172a',
          950: '#090d16',
        }
      },
    },
  },
  plugins: [],
}
