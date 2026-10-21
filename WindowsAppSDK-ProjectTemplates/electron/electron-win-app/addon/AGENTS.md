# Native Windows Addon (C++/WinRT)

C++ addon using N-API and WinRT. Provides Windows APIs not available in Node.js.

## Key files
- `addon.cc`: C++ implementation with WinRT calls
- `binding.gyp`: node-gyp build configuration

## When to modify
- Adding new Windows/WinAppSDK API calls
- Exposing new native methods to main process

## Dependencies
- Headers: `.winapp/include/`
- Libraries: `.winapp/lib/`
- Run `winapp restore` if missing

*For implementation patterns and debugging, see root [AGENTS.md](../../AGENTS.md).*
