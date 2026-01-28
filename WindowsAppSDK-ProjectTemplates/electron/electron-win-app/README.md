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

To extend Windows integration:
1. Add C++ code in `addon/` to expose WinRT APIs
2. Update `addon/binding.gyp` with dependencies
3. Run `npm run build-addon` to rebuild
4. Access the addon from Electron main process

See `AGENTS.md` for detailed architecture and development guidance.

## Troubleshooting

- **Build errors**: Ensure Visual Studio C++ tools and Windows SDK are installed
- **Certificate issues**: Run `npm run clean-debug` and `npm run setup-debug`
- **Hot reload not working**: Only React changes hot reload; C++ changes require `npm run build-addon` and restart

## Resources

- [Electron Documentation](https://www.electronjs.org/docs)
- [WinApp CLI](https://github.com/microsoft/winappcli)
- [Windows App SDK](https://docs.microsoft.com/windows/apps/windows-app-sdk/)
- [Node-gyp Guide](https://github.com/nodejs/node-gyp)
