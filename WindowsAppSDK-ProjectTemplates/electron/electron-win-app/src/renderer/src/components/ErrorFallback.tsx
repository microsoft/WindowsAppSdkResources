import type { FallbackProps } from 'react-error-boundary'

export function ErrorFallback({ error, resetErrorBoundary }: FallbackProps): React.JSX.Element {
  const errorMessage = error instanceof Error ? error.message : String(error)
  return (
    <div className="error-fallback">
      <h2>⚠️ Something went wrong</h2>
      <pre className="error-message">{errorMessage}</pre>
      <button className="primary" onClick={resetErrorBoundary}>
        Try again
      </button>
    </div>
  )
}
