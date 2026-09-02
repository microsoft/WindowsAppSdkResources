# Electron Windows App

An Electron application template with React, TypeScript, and Windows native capabilities through C++ addons and sparse MSIX packaging.

## What's Included

- **Electron + React + TypeScript**: Modern web development with hot reload
- **Native Windows Integration**: C++ addon (node-gyp) for accessing Windows Runtime (WinRT) APIs
- **Sparse MSIX Packaging**: Provides Package Identity for Windows notifications, protocols, and other identity-required features
- **WinApp CLI**: Simplified workflows for Windows app development and packaging

## Prerequisites

- Node.js (LTS version recommended)
- Visual Studio 2019 or later with C++ build tools
- Windows 10 SDK (10.0.19041.0 or later)

## Recommended IDE Setup

- [VS Code](https://code.visualstudio.com/) + [ESLint](https://marketplace.visualstudio.com/items?itemName=dbaeumer.vscode-eslint) + [Prettier](https://marketplace.visualstudio.com/items?itemName=esbenp.prettier-vscode)

## Quick Start

### First-Time Setup

```bash
# Install dependencies, build native addon, and setup debug identity
npm install
```

The `postinstall` script automatically:
- Restores WinApp packages
- Generates a development certificate
- Builds the C++ addon
- Sets up Electron debug identity

### Development

```bash
# Start with hot reload (React only)
npm run dev
```

For C++ addon changes:
```bash
# Rebuild the native addon
npm run build-addon

# Then restart dev server
npm run dev
```

## Build Commands

### Development Builds

```bash
# Build Electron app only
npm run build

# Build everything (addon + Electron)
npm run build-all

# Build unpacked for testing (x64)
npm run build:unpack

# Build unpacked for ARM64
npm run build:unpack:arm64
```

### MSIX Package

```bash
# Package as MSIX (x64)
npm run package-msix

# Package as MSIX (ARM64)
npm run package-msix:arm64
```

The MSIX packages will be created in the `./dist` folder, signed with the development certificate.

## Project Structure

```
electron-win-app/
├── src/
│   ├── main/           # Electron main process (Node.js)
│   ├── preload/        # Preload scripts (bridge)
│   └── renderer/       # React frontend
├── addon/              # C++ native addon
│   └── binding.gyp     # Node-gyp configuration
├── build/              # Native Windows components
├── Assets/             # App icons and assets
├── appxmanifest.xml    # MSIX manifest
├── devcert.pfx         # Development certificate (auto-generated)
└── winapp.yaml         # WinApp CLI configuration
```

## Useful Commands

```bash
# Format code
npm run format

# Lint code
npm run lint

# Type check
npm run typecheck

# Clean build artifacts
npm run clean

# Setup debug identity (run after clean)
npm run setup-debug

# Clear debug identity
npm run clean-debug
```

## Adding Windows Features

This template uses a C++ native addon to access Windows APIs (WinRT). When you need to use a Windows API that isn't available in JavaScript, follow these steps:

### Step 1: Add C++ function in addon

Edit [addon/addon.cc](addon/addon.cc) to add your new Windows API wrapper:

```cpp
#include <winrt/Windows.System.h>  // Add necessary WinRT headers

void YourNewFunction(const Napi::CallbackInfo& info) {
    Napi::Env env = info.Env();
    
    // Get arguments from JavaScript
    std::string arg1 = info[0].As<Napi::String>().Utf8Value();
    
    // Call WinRT API
    winrt::init_apartment();
    // ... your WinRT code here ...
    
    // Return result to JavaScript
    return Napi::String::New(env, "result");
}

// Register in Init() function at the bottom of the file
exports.Set("yourNewFunction", Napi::Function::New(env, YourNewFunction));
```

### Step 2: Update binding.gyp (if needed)

If your API requires additional libraries, edit [addon/binding.gyp](addon/binding.gyp):

```json
"libraries": [
  "WindowsApp.lib",
  "YourAdditionalLib.lib"  // Add new libs here
]
```

### Step 3: Rebuild the addon

```bash
npm run build-addon
```

### Step 4: Add IPC handler in main process

Edit [src/main/index.ts](src/main/index.ts):

```typescript
ipcMain.handle('your-new-method', (_event, arg1) => {
  return nativeAddon.yourNewFunction(arg1)
})
```

### Step 5: Call from renderer

```typescript
const result = await window.electron.ipcRenderer.invoke('your-new-method', arg1)
```

### Common Windows APIs Examples

| Feature | WinRT Namespace | Example Use |
|---------|-----------------|-------------|
| Notifications | `Windows.UI.Notifications` | Toast notifications |
| File Picker | `Windows.Storage.Pickers` | Native file dialogs |
| Share | `Windows.ApplicationModel.DataTransfer` | Share content |
| Clipboard | `Windows.ApplicationModel.DataTransfer` | Advanced clipboard |
| System Info | `Windows.System` | Device info, memory |

See [AGENTS.md](AGENTS.md) for detailed architecture and more examples.

## Troubleshooting

- **Build errors**: Ensure Visual Studio C++ tools and Windows SDK are installed
- **Certificate issues**: Run `npm run clean-debug` and `npm run setup-debug`
- **Hot reload not working**: Only React changes hot reload; C++ changes require `npm run build-addon` and restart

## Resources

- [Electron Documentation](https://www.electronjs.org/docs)
- [WinApp CLI](https://github.com/microsoft/winappcli)
- [Windows App SDK](https://docs.microsoft.com/windows/apps/windows-app-sdk/)
- [Node-gyp Guide](https://github.com/nodejs/node-gyp)
