export const defaultReturnTo = '/wardrobe'

const sessionPaths = ['/welcome', '/callback']

/** A fixed, unreachable origin used only to resolve `value` as a URL without hitting the network. */
const placeholderOrigin = 'https://buckl.invalid'

/**
 * Where to land after signing in. Only a path inside the app is accepted, so a crafted value can
 * never send the user to another site (`//evil.example`, `https://…`, or a value the URL parser
 * would strip control characters from and resolve off-site), and `/welcome` or `/callback` in any
 * case or with a trailing slash never send the user back into the sign-in screens in a loop.
 */
export function safeReturnTo(value: unknown): string {
  if (typeof value !== 'string' || !value.startsWith('/') || value.includes('\\')) {
    return defaultReturnTo
  }
  if (hasWhitespaceOrControlCharacter(value)) return defaultReturnTo

  let url: URL
  try {
    url = new URL(value, placeholderOrigin)
  } catch {
    return defaultReturnTo
  }
  if (url.origin !== placeholderOrigin) return defaultReturnTo

  const path = url.pathname.replace(/\/+$/, '').toLowerCase() || '/'
  return sessionPaths.includes(path) ? defaultReturnTo : value
}

/** Whitespace (including tab, newline and carriage return) and other control characters: the URL
 * parser silently strips some of these, which can turn an in-app-looking path into an off-site
 * redirect. Checked over code points, not a regex, because ESLint's `no-control-regex` flags a
 * control character written inside a regex literal. */
function hasWhitespaceOrControlCharacter(value: string): boolean {
  for (let i = 0; i < value.length; i++) {
    const code = value.charCodeAt(i)
    if (code <= 0x20 || code === 0x7f) return true
  }
  return false
}
