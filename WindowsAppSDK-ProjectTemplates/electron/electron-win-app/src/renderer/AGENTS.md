# Renderer Process (React Frontend)

## Purpose
- React frontend running in Chromium renderer process
- UI components, state management, user interactions
- Communicates with main process via IPC

## Structure
```
renderer/
├── src/
│   ├── App.tsx              # Main app component
│   ├── main.tsx             # React entry point with ErrorBoundary
│   ├── assets/              # CSS, images, fonts
│   ├── components/          # Reusable UI components
│   │   └── ErrorFallback.tsx
│   └── utils/
│       └── logger.ts        # Logger utility for terminal output
```

## IPC usage
```typescript
// Call main process
const result = await window.electron.ipcRenderer.invoke('method-name', arg1, arg2)

// Listen for events from main
window.electron.ipcRenderer.on('event-name', (_event, data) => {
  // Handle event
})
```

## Logging (IMPORTANT)
**All logs go to the terminal**, not just DevTools:
```typescript
import { logger } from './utils/logger'

logger.info('feature', 'User clicked button')
logger.warn('validation', 'Input too long')
logger.error('api', 'Request failed', { status: 500 })
```

Output in terminal: `[timestamp] [Renderer:feature] User clicked button`

**AI Agents: Use `get_terminal_output` to check logs when debugging.**

## Error handling
- Global error handlers in `main.tsx` catch all unhandled errors
- React ErrorBoundary catches component errors
- All errors are forwarded to terminal via IPC

## Hot reload
- Changes to `.tsx`, `.css` files auto-reload via Vite HMR
- No need to restart app for renderer-only changes
- State may reset on hot reload

## When to modify
- Adding UI features, components, pages
- Handling user input and displaying data
- Calling main process for native operations

## Debugging
- **Terminal first** - all logs and errors appear there
- Press F12 to open DevTools for DOM/Network inspection
- React DevTools extension for component inspection

## Do NOT
- Import Node.js modules directly (use preload/IPC)
- Access `window.chrome.webview` (that's for WebView2, not Electron)
- Call native addon directly (must go through main process)
