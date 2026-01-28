/**
 * Logger utility that sends logs to main process terminal via IPC
 */

type LogLevel = 'info' | 'warn' | 'error'

interface LogData {
  type: string
  message: string
  [key: string]: unknown
}

function sendLog(level: LogLevel, data: LogData): void {
  // Log to console for DevTools
  const consoleFn =
    level === 'error' ? console.error : level === 'warn' ? console.warn : console.log
  consoleFn(`[${data.type}]`, data.message)

  // Forward to main process terminal
  window.electron.ipcRenderer.send('renderer-log', {
    level,
    timestamp: new Date().toISOString(),
    ...data
  })
}

export const logger = {
  info: (type: string, message: string, extra?: Record<string, unknown>): void => {
    sendLog('info', { type, message, ...extra })
  },

  warn: (type: string, message: string, extra?: Record<string, unknown>): void => {
    sendLog('warn', { type, message, ...extra })
  },

  error: (type: string, message: string, extra?: Record<string, unknown>): void => {
    sendLog('error', { type, message, ...extra })
  }
}

/**
 * Setup global error handlers that forward errors to main process
 */
export function setupGlobalErrorHandlers(): void {
  window.onerror = (message, source, line, col, error) => {
    logger.error('global', String(message), {
      source,
      line,
      col,
      stack: error?.stack
    })
    return false
  }

  window.onunhandledrejection = (event) => {
    logger.error('unhandledrejection', String(event.reason), {
      stack: event.reason?.stack
    })
  }
}

/**
 * Error handler for React ErrorBoundary
 */
export function handleReactError(error: unknown, info: { componentStack?: string | null }): void {
  const errorMessage = error instanceof Error ? error.message : String(error)
  const errorStack = error instanceof Error ? error.stack : undefined
  logger.error('react', errorMessage, {
    stack: errorStack,
    componentStack: info.componentStack
  })
}
