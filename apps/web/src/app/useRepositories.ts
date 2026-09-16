import { useContext } from 'react'
import { RepositoriesContext, type Repositories } from './RepositoriesContext'

export function useRepositories(): Repositories {
  const repositories = useContext(RepositoriesContext)
  if (!repositories) {
    throw new Error('useRepositories must be used inside a RepositoriesProvider.')
  }
  return repositories
}
