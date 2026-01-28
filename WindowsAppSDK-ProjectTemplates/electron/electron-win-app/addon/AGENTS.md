# Native Windows Addon (C++/WinRT)

## Purpose
- C++ native addon using N-API and WinRT
- Provides access to Windows APIs not available in Node.js
- Called from main process via `require()`

## Key files
- `addon.cc`: C++ implementation with WinRT calls
- `binding.gyp`: node-gyp build configuration

## Build command
```bash
npm run build-addon
# or directly:
node-gyp clean configure build --directory=addon
```

## Adding a new function

### 1. Implement in C++
```cpp
Napi::Value YourFunction(const Napi::CallbackInfo& info) {
    Napi::Env env = info.Env();
    
    // Validate arguments
    if (info.Length() < 1 || !info[0].IsString()) {
        Napi::TypeError::New(env, "String expected").ThrowAsJavaScriptException();
        return env.Null();
    }
    
    // Get arguments
    std::string arg = info[0].As<Napi::String>();
    
    try {
        // Call WinRT APIs
        // ...
        
        return Napi::String::New(env, "result");
    } catch (const winrt::hresult_error& ex) {
        Napi::Error::New(env, winrt::to_string(ex.message())).ThrowAsJavaScriptException();
        return env.Null();
    }
}
```

### 2. Register in Init()
```cpp
Napi::Object Init(Napi::Env env, Napi::Object exports) {
    exports.Set("yourFunction", Napi::Function::New(env, YourFunction));
    return exports;
}
```

### 3. Rebuild
```bash
npm run build-addon
```

## WinRT usage patterns

### Include headers
```cpp
#include <winrt/Windows.Foundation.h>
#include <winrt/Windows.UI.Notifications.h>
// Add more as needed from .winapp/include/
```

### Async operations
- WinRT async operations need careful handling in N-API
- Consider using `Napi::AsyncWorker` for long operations

## Dependencies
- Headers: `.winapp/include/` (cppwinrt, Windows SDK)
- Libraries: `.winapp/lib/x64/` (WindowsApp.lib, Bootstrap.lib)
- Run `winapp restore` if missing

## Debugging
- Debug symbols already configured in `binding.gyp` (`GenerateDebugInformation: true`)
- Run `npm run dev` to start the app
- Attach Visual Studio → Debug → Attach to Process → `electron.exe`
- Set breakpoints in `addon.cc`
- Addon exceptions are caught in main process and logged to **terminal**

**AI Agents: Check terminal output for addon errors - they appear as `console.error` from main process.**

## Common errors
| Error | Cause | Fix |
|-------|-------|-----|
| LNK1181 cannot open .lib | Missing libraries | Run `winapp restore` |
| Cannot find header | Missing includes | Check `.winapp/include/` |
| WinRT exception | Missing Package Identity | Run `npm run setup-debug` |
