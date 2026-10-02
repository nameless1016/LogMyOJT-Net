/** @type {import('tailwindcss').Config} */
module.exports = {
  // tailwind looks through every razor file for class names
  content: ["./Components/**/*.{razor,html,cs}"],
  theme: {
    extend: {
      colors: {
        // these read css variables so dark mode only has to swap the variables
        page: "rgb(var(--page) / <alpha-value>)",
        surface: "rgb(var(--surface) / <alpha-value>)",
        bar: "rgb(var(--bar) / <alpha-value>)",
        fg: "rgb(var(--fg) / <alpha-value>)",
        body: "rgb(var(--body) / <alpha-value>)",
        soft: "rgb(var(--soft) / <alpha-value>)",
        muted: "rgb(var(--muted) / <alpha-value>)",
        line: "rgb(var(--line) / <alpha-value>)",
        accent: "rgb(var(--accent) / <alpha-value>)",
        "accent-dark": "rgb(var(--accent-dark) / <alpha-value>)",
        ok: "rgb(var(--ok) / <alpha-value>)",
        danger: "rgb(var(--danger) / <alpha-value>)",
        cream: "#faf6ed",
      },
      fontFamily: {
        serif: ["Fraunces", "serif"],
        sans: ["Inter", "sans-serif"],
        mono: ['"IBM Plex Mono"', "monospace"],
      },
      boxShadow: {
        soft: "0 1px 2px rgba(27, 42, 65, 0.06), 0 4px 16px rgba(27, 42, 65, 0.05)",
        pop: "0 8px 24px rgba(27, 42, 65, 0.18)",
      },
    },
  },
  plugins: [],
};
