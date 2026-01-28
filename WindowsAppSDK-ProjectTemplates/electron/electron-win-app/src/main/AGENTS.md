# Main Process

## Purpose
- Electron main process running in Node.js
- Handles IPC from renderer, manages windows, loads native addon

## Key files
- `index.ts`: Main entry point, window creation, IPC handlers

## When to modify
- Adding new IPC handlers for renderer communication
- Adding new native addon method calls
- Changing window behavior or app lifecycle

## IPC handler pattern
```typescript
ipcMain.handle('method-name', async (_event, arg1: Type1, arg2: Type2) => {
  try {
    const result = nativeAddon.someMethod(arg1, arg2)
    return { success: true, data: result }
  } catch (error) {
    console.error('Method failed:', error)
    return { success: false, error: String(error) }
  }
})
```

## Native addon access
```typescript
// Load at top of file
const nativeAddon = require('../../nativeWindowsAddon/build/Release/nativeWindowsAddon.node')

// Call methods
nativeAddon.showNotification(title, message)
```

## Debugging
- Console.log appears in terminal where `npm run dev` runs
- Use `console.error` for errors
- Attach debugger via VS Code "Attach to Process"

## Build & Run
- Changes require restart: Ctrl+C then `npm run dev`
- No hot reload for main process
