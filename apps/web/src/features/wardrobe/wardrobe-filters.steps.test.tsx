import { describeFeature, loadFeature } from '@amiceli/vitest-cucumber'
import { cleanup, screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { expect } from 'vitest'
import { InMemoryGarmentRepository } from '../../data/in-memory-garment-repository'
import { InMemoryProductRepository } from '../../data/in-memory-product-repository'
import type { Garment } from '../../domain/garment'
import type { Product } from '../../domain/product'
import { GarmentMother, ProductMother } from '../../test/garment-mother'
import { renderApp } from '../../test/render-app'

const feature = await loadFeature('./wardrobe-filters.feature')

/** The three garments every scenario starts with; the blue top may link to a product. */
const baseGarments = (productId: string | null = null): Garment[] => [
  GarmentMother.active({
    id: 'blue-top',
    productId,
    classification: { category: 'top', color: 'blue', size: 'M' },
  }),
  GarmentMother.active({
    id: 'black-bottom',
    classification: { category: 'bottom', color: 'black', size: '32' },
  }),
  GarmentMother.active({
    id: 'red-dress',
    classification: { category: 'dress', color: 'red', size: 'S' },
  }),
]

describeFeature(feature, ({ Background, Scenario, ScenarioOutline, AfterEachScenario }) => {
  AfterEachScenario(() => cleanup())

  let products: Product[]
  let repository: InMemoryGarmentRepository
  const user = userEvent.setup()

  const openWardrobe = (route = '/wardrobe') =>
    renderApp({
      route,
      repositories: {
        garments: repository,
        products: new InMemoryProductRepository(products),
      },
    })

  const expectOnlyListed = async (title: string) => {
    const list = await screen.findByRole('list', { name: 'Garments' })
    await waitFor(() => expect(within(list).getAllByRole('listitem')).toHaveLength(1))
    expect(within(list).getByRole('img', { name: new RegExp(`^${title}$`, 'i') })).toBeVisible()
  }

  Background(({ Given, And }) => {
    Given(
      'a wardrobe with a blue top sized M, a black bottom sized 32 and a red dress sized S',
      () => {
        products = []
        repository = new InMemoryGarmentRepository(baseGarments(), { products })
      },
    )
    And(
      'the blue top comes from the product {string} by {string}',
      (_ctx, name: string, brand: string) => {
        const product = ProductMother.fromUrl({ id: 'oxford', name, brand })
        products = [product]
        repository = new InMemoryGarmentRepository(baseGarments(product.id), { products })
      },
    )
  })

  ScenarioOutline('filtering by category keeps only that category', ({ When, Then }, variables) => {
    When('the user filters the wardrobe by category "<category>"', async () => {
      openWardrobe()
      await user.selectOptions(await screen.findByLabelText('Category'), variables.category)
    })
    Then('only the <expected> is listed', async () => {
      await expectOnlyListed(variables.expected)
    })
  })

  Scenario('filtering by color keeps only that color', ({ When, Then }) => {
    When('the user filters the wardrobe by color {string}', async (_ctx, color: string) => {
      openWardrobe()
      await user.selectOptions(await screen.findByLabelText('Color'), color)
    })
    Then('only the black bottom is listed', () => expectOnlyListed('Black bottom'))
  })

  Scenario('filtering by size keeps only that size', ({ When, Then }) => {
    When('the user filters the wardrobe by size {string}', async (_ctx, size: string) => {
      openWardrobe()
      await user.type(await screen.findByLabelText('Size'), size)
    })
    Then('only the red dress is listed', () => expectOnlyListed('Red dress'))
  })

  Scenario('searching by a word matches the linked product', ({ When, Then }) => {
    When('the user searches the wardrobe for {string}', async (_ctx, text: string) => {
      openWardrobe()
      await user.type(await screen.findByLabelText('Search'), text)
    })
    Then('only the blue top is listed', () => expectOnlyListed('Blue top'))
  })

  Scenario('filters live in the address so they survive a reload', ({ When, Then }) => {
    When('the user opens the wardrobe at {string}', (_ctx, route: string) => {
      openWardrobe(route)
    })
    Then('only the red dress is listed', () => expectOnlyListed('Red dress'))
  })

  Scenario('clearing the filters shows the whole wardrobe again', ({ Given, When, Then }) => {
    Given('the wardrobe is filtered by category {string}', async (_ctx, category: string) => {
      openWardrobe(`/wardrobe?category=${category.toLowerCase()}`)
      await expectOnlyListed('Blue top')
    })
    When('the user clears the filters', async () => {
      await user.click(screen.getByRole('button', { name: 'Clear filters' }))
    })
    Then('all three garments are listed', async () => {
      const list = await screen.findByRole('list', { name: 'Garments' })
      await waitFor(() => expect(within(list).getAllByRole('listitem')).toHaveLength(3))
    })
  })

  Scenario('no garment matches the filters', ({ When, Then, And }) => {
    When('the user filters the wardrobe by color {string}', async (_ctx, color: string) => {
      openWardrobe()
      await user.selectOptions(await screen.findByLabelText('Color'), color)
    })
    Then('a message says no garment matches the filters', async () => {
      expect(
        await screen.findByRole('region', { name: 'No garment matches these filters' }),
      ).toBeVisible()
    })
    And('a way to show the whole wardrobe is offered', () => {
      expect(screen.getByRole('button', { name: 'Show the whole wardrobe' })).toBeVisible()
    })
  })
})
