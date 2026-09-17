import { ElectronAPI } from '@electron-toolkit/preload'

interface NativeAPI {
  showNotification: (
    title: string,
    message: string
  ) => Promise<{ success: boolean; error?: string }>
}

declare global {
  interface Window {
    electron: ElectronAPI
    api: NativeAPI
  }
}
