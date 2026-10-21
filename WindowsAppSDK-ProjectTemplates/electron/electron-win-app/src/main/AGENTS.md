# Main Process

Electron main process (Node.js) - handles IPC, manages windows, loads native addon.

## Key files
- `index.ts`: Entry point, window creation, IPC handlers

## When to modify
- Adding new IPC handlers for renderer communication
- Adding new native addon method calls
- Changing window behavior or app lifecycle

*For IPC patterns and debugging, see root [AGENTS.md](../../AGENTS.md).*
