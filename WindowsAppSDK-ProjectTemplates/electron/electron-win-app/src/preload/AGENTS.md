# Preload Scripts

Bridge between main (Node.js) and renderer (Chromium). Exposes safe APIs via `contextBridge`.

## Key files
- `index.ts`: Exposes `window.electron`
- `index.d.ts`: TypeScript declarations

## When to modify
- Exposing new IPC channels to renderer
- Adding new safe APIs for renderer access

## Do NOT
- Expose `require()` or sensitive Node.js APIs
- Disable `contextIsolation` or `sandbox`
