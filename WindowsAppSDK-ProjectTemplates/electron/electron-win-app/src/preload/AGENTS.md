# Preload Scripts

## Purpose
- Bridge between main process (Node.js) and renderer process (Chromium)
- Exposes safe APIs to renderer via `contextBridge`
- Runs in isolated context with access to Node.js APIs

## Key files
- `index.ts`: Main preload script, exposes `window.electron`
- `index.d.ts`: TypeScript declarations for exposed APIs

## Security model
```typescript
// Preload exposes limited APIs
contextBridge.exposeInMainWorld('electron', {
  ipcRenderer: {
    invoke: (channel, ...args) => ipcRenderer.invoke(channel, ...args),
    on: (channel, callback) => ipcRenderer.on(channel, callback)
  }
})
```

## When to modify
- Exposing new IPC channels to renderer
- Adding new safe APIs for renderer access
- Changing what renderer can access

## Do NOT
- Expose full `require()` to renderer
- Expose sensitive Node.js APIs
- Disable `contextIsolation` or `sandbox`
