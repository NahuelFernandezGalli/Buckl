export const defaultReturnTo = '/wardrobe'

const sessionPaths = ['/welcome', '/callback']

/**
 * Where to land after signing in. Only a path inside the app is accepted, so a crafted value can
 * never send the user to another site (`//evil.example`, `https://…`) or back into the sign-in
 * screens in a loop.
 */
export function safeReturnTo(value: unknown): string {
  if (typeof value !== 'string' || !value.startsWith('/') || value.startsWith('//')) {
    return defaultReturnTo
  }
  if (value.includes('\\')) return defaultReturnTo
  const path = value.split(/[?#]/, 1)[0]
  return sessionPaths.includes(path) ? defaultReturnTo : value
}
