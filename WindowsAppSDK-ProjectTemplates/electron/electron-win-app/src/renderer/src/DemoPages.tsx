import { useState } from 'react'

interface NativeResult<T = unknown> {
  loading: boolean
  data: T | null
  error: string | null
}

function useNativeCall<T>(handler: () => Promise<T>): NativeResult<T> & { call: () => void } {
  const [state, setState] = useState<NativeResult<T>>({
    loading: false,
    data: null,
    error: null
  })

  const call = async (): Promise<void> => {
    setState({ loading: true, data: null, error: null })
    try {
      const data = await handler()
      setState({ loading: false, data, error: null })
    } catch (err) {
      setState({ loading: false, data: null, error: String(err) })
    }
  }

  return { ...state, call }
}

function pretty(v: unknown): string {
  if (v instanceof Error) return v.message
  if (typeof v === 'string') return v
  return JSON.stringify(v, null, 2)
}

function HomePage(): React.JSX.Element {
  const [notifTitle, setNotifTitle] = useState('Hello from Electron!')
  const [notifMessage, setNotifMessage] = useState('Native Windows notification via WinRT')

  const notification = useNativeCall(async () => {
    return await window.electron.ipcRenderer.invoke('show-notification', notifTitle, notifMessage)
  })

  const ipcPing = useNativeCall(async () => {
    window.electron.ipcRenderer.send('ping')
    return { sent: true, timestamp: new Date().toISOString() }
  })

  return (
    <div className="cards">
      <h2>Native Bridge Demo</h2>
      <p>Call native Windows APIs through Electron IPC + C++ addon.</p>

      <div className="card">
        <h3>🔔 Windows Notification</h3>
        <p className="card-desc">Send a native toast notification using WinRT APIs</p>
        <input
          className="text"
          placeholder="Notification title"
          value={notifTitle}
          onChange={(e) => setNotifTitle(e.target.value)}
        />
        <input
          className="text"
          placeholder="Notification message"
          value={notifMessage}
          onChange={(e) => setNotifMessage(e.target.value)}
        />
        <button className="primary" disabled={notification.loading} onClick={notification.call}>
          Show Notification
        </button>
        <h4>Result</h4>
        <pre className="output">
          {notification.error ? `Error: ${notification.error}` : pretty(notification.data)}
        </pre>
      </div>

      <div className="card">
        <h3>📡 IPC Communication</h3>
        <p className="card-desc">Test Electron IPC between renderer and main process</p>
        <button className="primary" disabled={ipcPing.loading} onClick={ipcPing.call}>
          Send Ping
        </button>
        <h4>Result</h4>
        <pre className="output">
          {ipcPing.error ? `Error: ${ipcPing.error}` : pretty(ipcPing.data)}
        </pre>
        <p className="hint">Check terminal for &quot;pong&quot; log from main process</p>
      </div>

      <div className="card">
        <h3>⚠️ Error Test</h3>
        <p className="card-desc">Test error handling - errors will appear in terminal</p>
        <div className="buttons">
          <button
            className="primary"
            onClick={() => {
              throw new Error('Test sync error from renderer!')
            }}
          >
            Throw Error
          </button>
          <button
            className="primary"
            onClick={() => {
              Promise.reject(new Error('Test unhandled promise rejection!'))
            }}
          >
            Unhandled Rejection
          </button>
        </div>
        <p className="hint">Check terminal for [Renderer error] logs</p>
      </div>

      <div className="card">
        <h3>🪪 Package Identity</h3>
        <p className="card-desc">This app runs with sparse MSIX package identity</p>
        <div className="info-box">
          <div className="info-item">
            <span className="label">Identity:</span>
            <code>electron-win-app.debug</code>
          </div>
          <div className="info-item">
            <span className="label">Publisher:</span>
            <code>CN=DEV</code>
          </div>
        </div>
        <p className="hint">Package identity enables Windows notifications and other modern APIs</p>
      </div>
    </div>
  )
}

function AboutPage(): React.JSX.Element {
  return (
    <div className="cards">
      <h2>About This Template</h2>
      <p>Electron + React + Native C++ Addon for Windows development</p>

      <div className="card wide">
        <h3>🏗️ Architecture</h3>
        <pre className="output architecture">
          {`┌─────────────────┐     IPC      ┌─────────────────┐     N-API     ┌─────────────────┐
│    Renderer     │◄────────────►│     Main        │◄─────────────►│   C++ Addon     │
│   (React UI)    │              │   (Node.js)     │               │    (WinRT)      │
└─────────────────┘              └─────────────────┘               └─────────────────┘`}
        </pre>
      </div>

      <div className="card">
        <h3>📁 Key Files</h3>
        <ul className="file-list">
          <li>
            <code>src/renderer/</code> - React frontend
          </li>
          <li>
            <code>src/main/</code> - Electron main process
          </li>
          <li>
            <code>src/preload/</code> - Context bridge
          </li>
          <li>
            <code>addon/addon.cc</code> - C++ native module
          </li>
        </ul>
      </div>

      <div className="card">
        <h3>🚀 Quick Commands</h3>
        <ul className="command-list">
          <li>
            <code>npm run dev</code> - Start with hot reload
          </li>
          <li>
            <code>npm run build-addon</code> - Rebuild C++ addon
          </li>
          <li>
            <code>npm run build:win</code> - Package for Windows
          </li>
        </ul>
      </div>

      <div className="card">
        <h3>📚 Resources</h3>
        <div className="buttons">
          <a
            className="link-button"
            href="https://electron-vite.org/"
            target="_blank"
            rel="noreferrer"
          >
            electron-vite Docs
          </a>
          <a
            className="link-button"
            href="https://nodejs.org/api/n-api.html"
            target="_blank"
            rel="noreferrer"
          >
            Node N-API
          </a>
          <a
            className="link-button"
            href="https://learn.microsoft.com/en-us/windows/apps/develop/"
            target="_blank"
            rel="noreferrer"
          >
            Windows Dev Docs
          </a>
        </div>
      </div>
    </div>
  )
}

export interface DemoTab {
  key: string
  label: string
  element: React.JSX.Element
}

export const demoTabs: DemoTab[] = [
  { key: 'home', label: 'Demo', element: <HomePage /> },
  { key: 'about', label: 'About', element: <AboutPage /> }
]

export { HomePage, AboutPage }
