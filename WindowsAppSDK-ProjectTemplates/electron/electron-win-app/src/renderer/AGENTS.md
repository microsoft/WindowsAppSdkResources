# Renderer Process (React Frontend)

React UI running in Chromium. Communicates with main process via IPC.

## Key files
- `App.tsx`: Main app component
- `main.tsx`: Entry point with ErrorBoundary
- `utils/logger.ts`: Logger for terminal output

## When to modify
- Adding UI features, components, pages
- Handling user input and displaying data

## Do NOT
- Import Node.js modules directly (use IPC)
- Call native addon directly (must go through main process)

*For IPC patterns and debugging, see root [AGENTS.md](../../AGENTS.md).*
