import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { App } from './app/App'
import { ConfigError, readAppConfig } from './app/config'
import './index.css'

const root = document.getElementById('root')!

try {
  // Fails fast, naming every missing variable, when apps/web/.env.local is not set up.
  const config = readAppConfig(import.meta.env)

  createRoot(root).render(
    <StrictMode>
      <App auth0={config.auth0} apiBaseUrl={config.apiBaseUrl} />
    </StrictMode>,
  )
} catch (error) {
  // A misconfigured deploy shows the problem on the page instead of leaving it blank
  // (docs/configuration.md): only a ConfigError has a message safe to display as-is.
  if (!(error instanceof ConfigError)) throw error
  root.textContent = error.message
}
