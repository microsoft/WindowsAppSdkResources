# Renderer Process (React Frontend)

React UI running in Chromium. Communicates with main process via IPC.

## Key files
- `App.tsx`: Main app component with `SHOW_DEMO` flag
- `DemoPages.tsx`: Demo pages (HomePage, AboutPage) - can be hidden when developing new features
- `main.tsx`: Entry point with ErrorBoundary
- `utils/logger.ts`: Logger for terminal output

## Hide demo code
When developing new features, set `SHOW_DEMO = false` in `App.tsx` to hide the demo pages. This keeps the demo code available for reference while giving you a clean slate for your own UI.

## When to modify
- Adding UI features, components, pages
- Handling user input and displaying data

## Do NOT
- Import Node.js modules directly (use IPC)
- Call native addon directly (must go through main process)

*For IPC patterns and debugging, see root [AGENTS.md](../../AGENTS.md).*
