import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { App } from './app/App'
import { readAppConfig } from './app/config'
import type { Repositories } from './app/RepositoriesContext'
import { InMemoryGarmentRepository } from './data/in-memory-garment-repository'
import { InMemoryProductRepository } from './data/in-memory-product-repository'
import { sampleGarments, sampleProducts } from './data/sample-wardrobe'
import './index.css'

// Fails fast, naming every missing variable, when apps/web/.env.local is not set up.
const config = readAppConfig(import.meta.env)

// Phase 3: the app runs against in-memory sample data. Phase 6 swaps these for API-backed repositories.
const repositories: Repositories = {
  garments: new InMemoryGarmentRepository(sampleGarments, { products: sampleProducts }),
  products: new InMemoryProductRepository(sampleProducts),
}

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <App auth0={config.auth0} repositories={repositories} />
  </StrictMode>,
)
