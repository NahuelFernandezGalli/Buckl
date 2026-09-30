import { describeFeature, loadFeature } from '@amiceli/vitest-cucumber'
import { cleanup, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { expect } from 'vitest'
import { SessionExpiredError } from '../session/session'
import { fakeSession, type FakeSession } from '../test/fake-session'
import { renderApp } from '../test/render-app'
import { ScriptedGarmentRepository } from '../test/scripted-garment-repository'

const feature = await loadFeature('./session-expiry.feature')

describeFeature(feature, ({ Scenario, AfterEachScenario }) => {
  AfterEachScenario(() => cleanup())

  const user = userEvent.setup()
  let session: FakeSession

  Scenario('an expired session offers to log in again and come back', ({ Given, Then, When }) => {
    Given('the session of the user expired while looking at their blue garments', () => {
      session = fakeSession()
      renderApp({ route: '/wardrobe?color=blue', session, sessionExpired: true })
    })
    Then('a notice says the session expired', async () => {
      expect(await screen.findByRole('alert')).toHaveTextContent('Your session expired.')
    })
    When('the user chooses to log in again', () =>
      user.click(screen.getByRole('button', { name: 'Log in again' })),
    )
    Then('the login comes back to the blue garments', () => {
      expect(session.signIns).toEqual(['/wardrobe?color=blue'])
    })
  })

  Scenario('a valid session shows no notice', ({ Given, Then }) => {
    Given('the user is looking at their wardrobe', () => {
      renderApp({ route: '/wardrobe' })
    })
    Then('no notice about the session is shown', async () => {
      expect(await screen.findByRole('heading', { level: 1, name: 'Wardrobe' })).toBeVisible()
      expect(screen.queryByRole('button', { name: 'Log in again' })).not.toBeInTheDocument()
    })
  })

  Scenario('a screen that cannot load does not repeat the notice', ({ Given, Then, And }) => {
    Given('the session of the user expired while the wardrobe was loading', () => {
      const garments = new ScriptedGarmentRepository()
      garments.failures.list.push(new SessionExpiredError())
      renderApp({ route: '/wardrobe', repositories: { garments }, sessionExpired: true })
    })
    Then('the notice is the only alert on the screen', async () => {
      // The failure has landed once the wardrobe stops loading.
      await waitFor(() => expect(screen.queryByRole('status')).not.toBeInTheDocument())
      expect(screen.getAllByRole('alert')).toHaveLength(1)
      expect(screen.getByRole('alert')).toHaveTextContent('Log in again')
    })
    And('there is nothing to try again', () => {
      expect(screen.queryByRole('button', { name: 'Try again' })).not.toBeInTheDocument()
    })
  })
})
