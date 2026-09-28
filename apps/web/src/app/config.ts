/** Public settings the bundle is built with. None of them is a secret (ADR-0003). */
export interface AppConfig {
  auth0: Auth0Settings
  apiBaseUrl: string
}

export interface Auth0Settings {
  /** Tenant host without scheme, such as `buckl-dev.us.auth0.com`. */
  domain: string
  clientId: string
  /** Identifier of the Buckl API in Auth0; access tokens carry it as `aud`. */
  audience: string
}

export type ConfigSource = Readonly<Record<string, string | boolean | undefined>>

const variables = {
  domain: 'VITE_AUTH0_DOMAIN',
  clientId: 'VITE_AUTH0_CLIENT_ID',
  audience: 'VITE_AUTH0_AUDIENCE',
  apiBaseUrl: 'VITE_API_BASE_URL',
} as const

export class ConfigError extends Error {
  constructor(problems: readonly string[]) {
    super(
      `Buckl is not configured: ${problems.join('; ')}. ` +
        'Copy apps/web/.env.example to apps/web/.env.local and fill it in.',
    )
    this.name = 'ConfigError'
  }
}

/** Reads and checks the `VITE_` variables. Every problem is reported at once. */
export function readAppConfig(source: ConfigSource): AppConfig {
  const problems: string[] = []
  const read = (name: string) => {
    const value = source[name]
    const text = typeof value === 'string' ? value.trim() : ''
    if (!text) problems.push(`${name} is missing`)
    return text
  }

  const domain = read(variables.domain)
  const clientId = read(variables.clientId)
  const audience = read(variables.audience)
  const apiBaseUrl = read(variables.apiBaseUrl)

  if (domain && !/^[a-z0-9-]+(\.[a-z0-9-]+)+$/i.test(domain)) {
    problems.push(`${variables.domain} must be a host name without https:// or slashes`)
  }
  if (apiBaseUrl && !isHttpUrl(apiBaseUrl)) {
    problems.push(`${variables.apiBaseUrl} must be an http or https URL`)
  }
  if (problems.length > 0) throw new ConfigError(problems)

  return { auth0: { domain, clientId, audience }, apiBaseUrl }
}

function isHttpUrl(value: string): boolean {
  try {
    const url = new URL(value)
    return url.protocol === 'https:' || url.protocol === 'http:'
  } catch {
    return false
  }
}
