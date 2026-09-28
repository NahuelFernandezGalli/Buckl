import { describe, expect, it } from 'vitest'
import { ConfigError, readAppConfig } from './config'

const complete = {
  VITE_AUTH0_DOMAIN: 'buckl-dev.us.auth0.com',
  VITE_AUTH0_CLIENT_ID: 'client-123',
  VITE_AUTH0_AUDIENCE: 'https://api.buckl.app',
  VITE_API_BASE_URL: 'http://localhost:5080',
}

describe('readAppConfig', () => {
  it('reads every setting, trimmed', () => {
    expect(readAppConfig({ ...complete, VITE_AUTH0_CLIENT_ID: '  client-123 ' })).toEqual({
      auth0: {
        domain: 'buckl-dev.us.auth0.com',
        clientId: 'client-123',
        audience: 'https://api.buckl.app',
      },
      apiBaseUrl: 'http://localhost:5080',
    })
  })

  it('reports every missing variable at once and says how to fix it', () => {
    const read = () => readAppConfig({ VITE_AUTH0_DOMAIN: 'buckl-dev.us.auth0.com' })

    expect(read).toThrow(ConfigError)
    expect(read).toThrow(
      /VITE_AUTH0_CLIENT_ID is missing; VITE_AUTH0_AUDIENCE is missing; VITE_API_BASE_URL is missing/,
    )
    expect(read).toThrow(/apps\/web\/\.env\.example/)
  })

  it('treats a blank value as missing', () => {
    expect(() => readAppConfig({ ...complete, VITE_AUTH0_AUDIENCE: '   ' })).toThrow(
      /VITE_AUTH0_AUDIENCE is missing/,
    )
  })

  it.each(['https://buckl-dev.us.auth0.com', 'buckl-dev.us.auth0.com/', 'localhost'])(
    'rejects %s as the Auth0 domain',
    (domain) => {
      expect(() => readAppConfig({ ...complete, VITE_AUTH0_DOMAIN: domain })).toThrow(
        /VITE_AUTH0_DOMAIN must be a host name/,
      )
    },
  )

  it.each(['localhost:5080', 'ftp://api.buckl.app'])('rejects %s as the API base URL', (url) => {
    expect(() => readAppConfig({ ...complete, VITE_API_BASE_URL: url })).toThrow(
      /VITE_API_BASE_URL must be an http or https URL/,
    )
  })

  it('rejects a plain http API base URL for a non-local host', () => {
    expect(() => readAppConfig({ ...complete, VITE_API_BASE_URL: 'http://api.buckl.app' })).toThrow(
      /VITE_API_BASE_URL must use https/,
    )
  })

  it.each(['http://localhost:5080', 'http://127.0.0.1:5080', 'https://api.buckl.app'])(
    'accepts %s as the API base URL',
    (url) => {
      expect(() => readAppConfig({ ...complete, VITE_API_BASE_URL: url })).not.toThrow()
    },
  )
})
