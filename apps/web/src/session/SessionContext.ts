import { createContext } from 'react'
import type { Session } from './session'

export const SessionContext = createContext<Session | null>(null)
