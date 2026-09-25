import type { Session, SessionState } from '../session/session'

/**
 * A session frozen in one state that records what the screens ask of it. It records in plain
 * arrays rather than `vi.fn()`: Vitest clears mock history before every test, and vitest-cucumber
 * runs each step as its own test, so a `Then` would never see the calls of its `When`.
 */
export interface FakeSession extends Session {
  /** The `returnTo` of every sign-in, in order. */
  readonly signIns: string[]
  readonly signOuts: { count: number }
}

export const signedInAsAlice: SessionState = {
  status: 'signedIn',
  user: { displayName: 'Alice' },
}

/** Signed in as Alice unless told otherwise. */
export function fakeSession(state: SessionState = signedInAsAlice): FakeSession {
  const signIns: string[] = []
  const signOuts = { count: 0 }
  return {
    state,
    signIns,
    signOuts,
    signIn: async (returnTo) => {
      signIns.push(returnTo)
    },
    signOut: async () => {
      signOuts.count += 1
    },
    getAccessToken: async () => 'test-access-token',
  }
}
