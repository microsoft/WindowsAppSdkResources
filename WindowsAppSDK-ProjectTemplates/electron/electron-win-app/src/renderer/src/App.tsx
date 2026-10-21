import { useState } from 'react'
import { demoTabs, DemoTab } from './DemoPages'

// Set to false to hide demo pages
const SHOW_DEMO = true

const tabs: DemoTab[] = SHOW_DEMO ? demoTabs : []

function App(): React.JSX.Element {
  const [active, setActive] = useState(tabs[0]?.key ?? '')

  return (
    <div className="app-shell">
      <header className="app-header">
        <div className="header-left">
          <span className="app-icon">⚡</span>
          <span className="app-title">Electron Windows App</span>
        </div>
        <nav className="tabs">
          {tabs.map((t) => (
            <button
              key={t.key}
              className={active === t.key ? 'tab active' : 'tab'}
              onClick={() => setActive(t.key)}
            >
              {t.label}
            </button>
          ))}
        </nav>
      </header>
      <main className="app-main">{tabs.find((t) => t.key === active)?.element}</main>
      <footer className="app-footer">
        <span>Electron + React + TypeScript + WinRT</span>
      </footer>
    </div>
  )
}

export default App
