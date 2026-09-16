import type { ReactNode } from 'react'
import { RepositoriesContext, type Repositories } from './RepositoriesContext'

export interface RepositoriesProviderProps {
  repositories: Repositories
  children: ReactNode
}

export function RepositoriesProvider({ repositories, children }: RepositoriesProviderProps) {
  return (
    <RepositoriesContext.Provider value={repositories}>{children}</RepositoriesContext.Provider>
  )
}
