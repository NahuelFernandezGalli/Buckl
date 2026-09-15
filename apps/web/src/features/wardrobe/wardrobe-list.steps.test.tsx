import { describeFeature, loadFeature } from '@amiceli/vitest-cucumber'
import { cleanup, screen, within } from '@testing-library/react'
import { expect } from 'vitest'
import { InMemoryGarmentRepository } from '../../data/in-memory-garment-repository'
import { GarmentMother } from '../../test/garment-mother'
import { renderApp } from '../../test/render-app'

const feature = await loadFeature('./wardrobe-list.feature')

const blueTop = () =>
  GarmentMother.active({
    id: 'blue-top',
    classification: { category: 'top', color: 'blue', size: 'M' },
  })

describeFeature(feature, ({ Scenario, AfterEachScenario }) => {
  AfterEachScenario(() => cleanup())

  let repository: InMemoryGarmentRepository

  const openWardrobe = () => {
    renderApp({ route: '/wardrobe', repositories: { garments: repository } })
  }

  Scenario('an empty wardrobe invites the first garment', ({ Given, When, Then }) => {
    Given('a wardrobe with no garments', () => {
      repository = new InMemoryGarmentRepository([])
    })
    When('the user opens the wardrobe', openWardrobe)
    Then('an invitation to add the first garment is shown', async () => {
      expect(await screen.findByRole('link', { name: 'Add your first garment' })).toHaveAttribute(
        'href',
        '/garments/new',
      )
    })
  })

  Scenario('garments are listed with their photo and category', ({ Given, When, Then, And }) => {
    Given('a wardrobe with a blue top and a black bottom', () => {
      repository = new InMemoryGarmentRepository([
        blueTop(),
        GarmentMother.active({
          id: 'black-bottom',
          classification: { category: 'bottom', color: 'black', size: '32' },
        }),
      ])
    })
    When('the user opens the wardrobe', openWardrobe)
    Then('both garments are listed', async () => {
      const list = await screen.findByRole('list', { name: 'Garments' })
      expect(within(list).getAllByRole('listitem')).toHaveLength(2)
    })
    And('each one shows its photo and its category', () => {
      const list = screen.getByRole('list', { name: 'Garments' })
      expect(within(list).getByRole('img', { name: 'Blue top' })).toBeVisible()
      expect(within(list).getByRole('img', { name: 'Black bottom' })).toBeVisible()
      expect(within(list).getByText('Top')).toBeVisible()
      expect(within(list).getByText('Bottom')).toBeVisible()
    })
  })

  Scenario('archived garments are not part of the wardrobe', ({ Given, When, Then }) => {
    Given('a wardrobe with an active blue top and an archived grey top', () => {
      repository = new InMemoryGarmentRepository([
        blueTop(),
        GarmentMother.archived({
          id: 'grey-top',
          classification: { category: 'top', color: 'grey', size: 'L' },
        }),
      ])
    })
    When('the user opens the wardrobe', openWardrobe)
    Then('only the blue top is listed', async () => {
      const list = await screen.findByRole('list', { name: 'Garments' })
      expect(within(list).getAllByRole('listitem')).toHaveLength(1)
      expect(within(list).getByRole('img', { name: 'Blue top' })).toBeVisible()
    })
  })

  Scenario('each garment leads to its detail', ({ Given, When, Then }) => {
    Given('a wardrobe with a blue top', () => {
      repository = new InMemoryGarmentRepository([blueTop()])
    })
    When('the user opens the wardrobe', openWardrobe)
    Then('the blue top links to its detail', async () => {
      const list = await screen.findByRole('list', { name: 'Garments' })
      expect(within(list).getByRole('link', { name: /blue top/i })).toHaveAttribute(
        'href',
        '/wardrobe/blue-top',
      )
    })
  })
})
