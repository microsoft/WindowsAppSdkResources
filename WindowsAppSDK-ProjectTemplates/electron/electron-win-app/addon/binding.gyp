{
  "targets": [
    {
      "target_name": "addon",
      "sources": ["addon.cc"],
      "include_dirs": [
        "<!@(node -p \"require('node-addon-api').include\")",
        "<!(node -e \"require('nan')\")",
        "../.winapp/include"
      ],
      "msvs_settings": {
        "VCCLCompilerTool": {
          "ExceptionHandling": 1,
          "AdditionalOptions": [
            "/FS"
          ]
        },
        "VCLinkerTool": {
          "GenerateDebugInformation": "true"
        }
      },
      "defines": [
        "NODE_ADDON_API_CPP_EXCEPTIONS",
        "WINVER=0x0A00",
        "_WIN32_WINNT=0x0A00"
      ],
      "library_dirs": [
        "../.winapp/lib/<(target_arch)"
      ],
      "libraries": [
        "comctl32.lib",
        "shcore.lib",
        "WindowsApp.lib",
        "Microsoft.WindowsAppRuntime.Bootstrap.lib"
      ],
      "dependencies": [
        "<!(node -p \"require('node-addon-api').gyp\")"
      ]
    }
  ]
}