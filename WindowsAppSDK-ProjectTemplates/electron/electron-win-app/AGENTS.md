# Electron Windows App (Electron + React + Native Addon)

**You are a desktop app developer** building an Electron + React app with C++/WinRT native addon. When implementing features, first check if Windows APIs (`Windows.*`) or Windows App SDK APIs (`Microsoft.*`) can be used before falling back to other solutions.

## Commands
- One-time setup: Run `npm install` (triggers `postinstall` which restores packages, builds addon, and sets up debug identity)
- Daily dev (hot reload): `npm run dev`
- Build addon after C++ changes: `npm run build-addon`
- Build everything: `npm run build-all` (addon + electron)
- Package MSIX: `npm run package-msix`

## AI Agent Task Loop

**For AI coding assistants: Follow these steps for feature implementations.**

### Where to implement

| If the feature involves... | Implement in |
|---------------------------|--------------|
| UI, user interaction, styling | `src/renderer/` (React) |
| File I/O, IPC handlers, app logic | `src/main/` (Node.js) |
| Windows/WinAppSDK APIs | `addon/addon.cc` (C++/WinRT) |

→ Use native addon when calling WinRT (`Windows.*`) or Windows App SDK (`Microsoft.*`) namespaces. See [Adding native Windows capabilities](#adding-native-windows-capabilities).

### Build → Test → Debug

1. **Build**: Native → `npm run build-addon`; Main → restart dev; Renderer → hot-reloads
2. **Test**: Run `npm run dev`, check terminal for logs/errors. See [Debugging](#debugging).

## Project Structure & Architecture

```
electron-win-app/
├── src/
│   ├── main/           # Main process (Node.js) - IPC handlers, app logic
│   ├── preload/        # Bridge: exposes safe IPC via contextBridge
│   └── renderer/       # Renderer process (React) - UI
├── addon/              # Native addon (C++/WinRT) - Windows APIs
├── .winapp/            # SDK packages, headers, libs (generated)
├── Assets/             # MSIX package assets
├── appxmanifest.xml    # MSIX manifest
└── winapp.yaml         # winapp CLI config
```

**Data flow:** Renderer → IPC → Main → N-API → Native Addon

*See `showNotification` in `addon/addon.cc` and `src/main/index.ts` for a working example.*

## Adding native Windows capabilities

### 1. Add function in C++ addon
Edit `addon/addon.cc`:
```cpp
void YourNewFunction(const Napi::CallbackInfo& info) {
    Napi::Env env = info.Env();
    // TODO: Get args, call WinRT APIs, return result
}

// Register in Init()
exports.Set("yourNewFunction", Napi::Function::New(env, YourNewFunction));
```

### 2. Rebuild addon
```bash
npm run build-addon
```

### 3. Add IPC handler in main process
Edit `src/main/index.ts`:
```typescript
ipcMain.handle('your-new-method', (_event, arg1, arg2) => {
  return nativeAddon.yourNewFunction(arg1, arg2)
})
```

### 4. Expose in preload (if needed)
Edit `src/preload/index.ts` to add to `contextBridge.exposeInMainWorld`

### 5. Call from renderer
```typescript
const result = await window.electron.ipcRenderer.invoke('your-new-method', arg1, arg2)
```

## Package Identity

Some Windows APIs require Package Identity (notifications, background tasks, etc.). Debug identity is set up automatically via `npm install`.

- Re-setup if needed: `npm run setup-debug`
- Clear (when removing project): `npm run clean-debug`

## Debugging

**Terminal is the primary log destination.** During `npm run dev`, all logs (main, renderer, native) appear in the terminal.

**Logging by layer:**
- **Renderer**: `import { logger } from './utils/logger'` → `logger.info('tag', 'msg')`
- **Main**: `console.log()` / `console.error()`
- **Native**: Throw `Napi::Error` or return error to main process

**AI Agents:** Check the terminal running `npm run dev` for all error messages.

### Common issues
| Issue | Solution |
|-------|----------|
| Identity-required API fails | Run `npm run setup-debug` |
| Addon build fails | Check `.winapp/lib/` exists, run `winapp restore` |
| IPC timeout | Check main process logs for errors |
