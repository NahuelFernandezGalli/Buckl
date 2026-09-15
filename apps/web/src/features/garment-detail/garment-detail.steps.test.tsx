import { describeFeature, loadFeature } from '@amiceli/vitest-cucumber'
import { cleanup, screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { expect } from 'vitest'
import { InMemoryGarmentRepository } from '../../data/in-memory-garment-repository'
import { InMemoryProductRepository } from '../../data/in-memory-product-repository'
import type { Garment } from '../../domain/garment'
import type { Product } from '../../domain/product'
import { GarmentMother, ProductMother } from '../../test/garment-mother'
import { renderApp } from '../../test/render-app'

const feature = await loadFeature('./garment-detail.feature')

const BLUE_TOP_ID = 'blue-top'

describeFeature(feature, ({ Scenario, AfterEachScenario }) => {
  AfterEachScenario(() => cleanup())

  let garments: Garment[]
  let products: Product[]

  const blueTop = (overrides: Partial<Garment> = {}) =>
    GarmentMother.active({
      id: BLUE_TOP_ID,
      classification: { category: 'top', color: 'blue', size: 'M' },
      ...overrides,
    })

  const open = (route: string) =>
    renderApp({
      route,
      repositories: {
        garments: new InMemoryGarmentRepository(garments, { products }),
        products: new InMemoryProductRepository(products),
      },
    })

  const openDetail = () => {
    open(`/wardrobe/${BLUE_TOP_ID}`)
  }

  Scenario('opening a garment from the wardrobe shows its detail', ({ Given, When, Then, And }) => {
    Given('a wardrobe with a blue top sized M', () => {
      garments = [blueTop()]
      products = []
    })
    When('the user opens the blue top from the wardrobe', async () => {
      open('/wardrobe')
      const list = await screen.findByRole('list', { name: 'Garments' })
      await userEvent.setup().click(within(list).getByRole('link', { name: /blue top/i }))
    })
    Then('the detail is titled {string}', async (_ctx, title: string) => {
      expect(await screen.findByRole('heading', { level: 1, name: title })).toBeVisible()
    })
    And(
      'the detail shows the photo, {string}, {string} and {string}',
      (_ctx, category: string, color: string, size: string) => {
        const detail = screen.getByRole('article')
        expect(within(detail).getByRole('img', { name: 'Blue top' })).toBeVisible()
        expect(within(detail).getByText(category)).toBeVisible()
        expect(within(detail).getByText(color)).toBeVisible()
        expect(within(detail).getByText(size)).toBeVisible()
      },
    )
  })

  Scenario(
    'purchase information shows the price with its currency and the date',
    ({ Given, When, Then, And }) => {
      Given('a blue top bought for {int} ARS on {string}', (_ctx, amount: number, date: string) => {
        garments = [blueTop({ purchaseInfo: { price: { amount, currency: 'ARS' }, date } })]
        products = []
      })
      When('the user opens the detail of the blue top', openDetail)
      Then('the price {string} is shown', async (_ctx, price: string) => {
        expect(await screen.findByText(price)).toBeVisible()
      })
      And('the purchase date {string} is shown', (_ctx, date: string) => {
        expect(screen.getByText(date)).toBeVisible()
      })
    },
  )

  Scenario('a garment without purchase information says so', ({ Given, When, Then }) => {
    Given('a blue top with no purchase information', () => {
      garments = [blueTop({ purchaseInfo: null })]
      products = []
    })
    When('the user opens the detail of the blue top', openDetail)
    Then('the detail says there is no purchase information', async () => {
      expect(await screen.findByText('No purchase information.')).toBeVisible()
    })
  })

  Scenario('a garment linked to a product shows the product', ({ Given, When, Then, And }) => {
    Given(
      'a blue top that comes from the product {string} by {string} at {string}',
      (_ctx, name: string, brand: string, url: string) => {
        const product = ProductMother.fromUrl({ id: 'oxford', name, brand, sourceUrl: url })
        products = [product]
        garments = [blueTop({ productId: product.id, source: 'url' })]
      },
    )
    When('the user opens the detail of the blue top', openDetail)
    Then('the product {string} by {string} is shown', async (_ctx, name: string, brand: string) => {
      const section = await screen.findByRole('region', { name: 'Product' })
      // Ruling R5: the product loads asynchronously after the section renders
      // ("Loading the product…" first), so await the text instead of getByText.
      expect(await within(section).findByText(name)).toBeVisible()
      expect(await within(section).findByText(brand)).toBeVisible()
    })
    And('a link to the product page at {string} is offered', (_ctx, url: string) => {
      const link = screen.getByRole('link', { name: 'View product page' })
      expect(link).toHaveAttribute('href', url)
      expect(link).toHaveAttribute('rel', expect.stringContaining('noopener'))
    })
  })

  Scenario('notes are shown', ({ Given, When, Then }) => {
    Given('a blue top with the note {string}', (_ctx, note: string) => {
      garments = [blueTop({ notes: note })]
      products = []
    })
    When('the user opens the detail of the blue top', openDetail)
    Then('the note {string} is shown', async (_ctx, note: string) => {
      const section = await screen.findByRole('region', { name: 'Notes' })
      expect(within(section).getByText(note)).toBeVisible()
    })
  })

  Scenario('an archived garment is marked as such', ({ Given, When, Then }) => {
    Given('an archived blue top', () => {
      garments = [GarmentMother.archived({ id: BLUE_TOP_ID })]
      products = []
    })
    When('the user opens the detail of the blue top', openDetail)
    Then('the detail is marked as archived', async () => {
      expect(await screen.findByText('Archived')).toBeVisible()
    })
  })

  Scenario('a garment that does not exist', ({ Given, When, Then, And }) => {
    Given('a wardrobe with no garments', () => {
      garments = []
      products = []
    })
    When('the user opens the detail of a garment that does not exist', () => {
      open('/wardrobe/missing')
    })
    Then('a message says the garment is not in the wardrobe', async () => {
      expect(
        await screen.findByRole('heading', { level: 1, name: 'Garment not found' }),
      ).toBeVisible()
    })
    And('a link back to the wardrobe is offered', () => {
      expect(screen.getByRole('link', { name: 'Back to the wardrobe' })).toHaveAttribute(
        'href',
        '/wardrobe',
      )
    })
  })
})
