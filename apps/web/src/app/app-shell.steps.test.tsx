import { describeFeature, loadFeature } from '@amiceli/vitest-cucumber'
import { cleanup, screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { expect } from 'vitest'
import { renderApp } from '../test/render-app'

const feature = await loadFeature('./app-shell.feature')

describeFeature(feature, ({ Scenario, AfterEachScenario }) => {
  AfterEachScenario(() => cleanup())

  Scenario('opening the app lands on the wardrobe', ({ When, Then }) => {
    When('the user opens the app', () => {
      renderApp({ route: '/' })
    })
    Then('the wardrobe is shown', async () => {
      expect(await screen.findByRole('heading', { level: 1, name: 'Wardrobe' })).toBeVisible()
    })
  })

  Scenario('the main navigation reaches the add garment screen', ({ Given, When, Then }) => {
    Given('the app is open on the wardrobe', () => {
      renderApp({ route: '/wardrobe' })
    })
    When('the user chooses {string} in the main navigation', async (_ctx, label: string) => {
      const navigation = screen.getByRole('navigation', { name: 'Main' })
      await userEvent.setup().click(within(navigation).getByRole('link', { name: label }))
    })
    Then('the add garment screen is shown', async () => {
      expect(await screen.findByRole('heading', { level: 1, name: 'Add garment' })).toBeVisible()
    })
  })

  Scenario('the main navigation returns to the wardrobe', ({ Given, When, Then }) => {
    Given('the app is open on the add garment screen', () => {
      renderApp({ route: '/garments/new' })
    })
    When('the user chooses {string} in the main navigation', async (_ctx, label: string) => {
      const navigation = screen.getByRole('navigation', { name: 'Main' })
      await userEvent.setup().click(within(navigation).getByRole('link', { name: label }))
    })
    Then('the wardrobe is shown', async () => {
      expect(await screen.findByRole('heading', { level: 1, name: 'Wardrobe' })).toBeVisible()
    })
  })

  Scenario('an unknown address shows a way back', ({ When, Then, And }) => {
    When('the user opens an address that does not exist', () => {
      renderApp({ route: '/nowhere' })
    })
    Then('a page not found message is shown', async () => {
      expect(await screen.findByRole('heading', { level: 1, name: 'Page not found' })).toBeVisible()
    })
    And('a link back to the wardrobe is offered', () => {
      expect(screen.getByRole('link', { name: 'Back to the wardrobe' })).toHaveAttribute(
        'href',
        '/wardrobe',
      )
    })
  })
})
