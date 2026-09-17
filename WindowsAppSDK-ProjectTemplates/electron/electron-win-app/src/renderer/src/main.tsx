import './assets/main.css'

import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { ErrorBoundary } from 'react-error-boundary'
import App from './App'
import { ErrorFallback } from './components/ErrorFallback'
import { setupGlobalErrorHandlers, handleReactError } from './utils/logger'

// Setup global error handlers to forward errors to main process terminal
setupGlobalErrorHandlers()

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <ErrorBoundary FallbackComponent={ErrorFallback} onError={handleReactError}>
      <App />
    </ErrorBoundary>
  </StrictMode>
)
