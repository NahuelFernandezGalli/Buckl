import { describeFeature, loadFeature } from '@amiceli/vitest-cucumber'
import { cleanup, render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { useState } from 'react'
import { expect, vi } from 'vitest'
import { PhotoCapture, type PhotoSelection } from './PhotoCapture'

const feature = await loadFeature('./photo-capture.feature')

function Harness({ initial = null }: { initial?: PhotoSelection | null }) {
  const [value, setValue] = useState<PhotoSelection | null>(initial)
  return <PhotoCapture value={value} onChange={setValue} />
}

const imageFile = () => new File(['photo'], 'shirt.jpg', { type: 'image/jpeg' })
const textFile = () => new File(['notes'], 'notes.txt', { type: 'text/plain' })

// Vitest 5 clears every mock's call history before each test, and vitest-cucumber runs each
// Gherkin step as its own test. A plain array survives that clearing, unlike `.mock.calls`, so
// steps that call revokeObjectURL and steps that assert on it (in a later step) can agree on it.
let revokedUrls: string[] = []

describeFeature(feature, ({ Scenario, AfterEachScenario, BeforeEachScenario }) => {
  BeforeEachScenario(() => {
    revokedUrls = []
    vi.mocked(URL.revokeObjectURL).mockImplementation((url) => {
      revokedUrls.push(url)
    })
  })
  AfterEachScenario(() => cleanup())

  const user = userEvent.setup()

  const expectPreview = () => {
    expect(screen.getByRole('img', { name: 'Garment photo preview' })).toHaveAttribute(
      'src',
      expect.stringMatching(/^blob:/),
    )
  }

  Scenario('taking a photo shows a preview', ({ Given, When, Then, And }) => {
    Given('the photo capture is shown', () => {
      render(<Harness />)
    })
    When('the user takes a photo with the camera', async () => {
      const camera = screen.getByLabelText('Take photo')
      expect(camera).toHaveAttribute('capture', 'environment')
      await user.upload(camera, imageFile())
    })
    Then('a preview of the photo is shown', expectPreview)
    And('the user can retake it', () => {
      expect(screen.getByRole('button', { name: 'Retake photo' })).toBeVisible()
    })
  })

  Scenario('choosing a photo from the gallery shows a preview', ({ Given, When, Then }) => {
    Given('the photo capture is shown', () => {
      render(<Harness />)
    })
    When('the user chooses a photo from the gallery', async () => {
      const gallery = screen.getByLabelText('Choose from gallery')
      expect(gallery).not.toHaveAttribute('capture')
      await user.upload(gallery, imageFile())
    })
    Then('a preview of the photo is shown', expectPreview)
  })

  Scenario('retaking discards the previous photo', ({ Given, When, Then, And }) => {
    Given('the photo capture is shown with a photo already taken', () => {
      render(<Harness initial={{ file: imageFile(), previewUrl: 'blob:preview-existing' }} />)
      expectPreview()
    })
    When('the user retakes the photo', async () => {
      await user.click(screen.getByRole('button', { name: 'Retake photo' }))
    })
    Then('no preview is shown', () => {
      expect(screen.queryByRole('img')).not.toBeInTheDocument()
    })
    And('the camera and the gallery are offered again', () => {
      expect(screen.getByLabelText('Take photo')).toBeInTheDocument()
      expect(screen.getByLabelText('Choose from gallery')).toBeInTheDocument()
    })
    And('the previous preview was released', () => {
      expect(revokedUrls).toContain('blob:preview-existing')
    })
  })

  Scenario('a file that is not an image is rejected', ({ Given, When, Then, And }) => {
    Given('the photo capture is shown', () => {
      render(<Harness />)
    })
    When('the user chooses a file that is not an image', async () => {
      // user-event honours the input's accept attribute by default; disable it to simulate a
      // browser or file picker that lets a non-image through.
      await userEvent
        .setup({ applyAccept: false })
        .upload(screen.getByLabelText('Choose from gallery'), textFile())
    })
    Then('a message says only images are accepted', () => {
      expect(screen.getByRole('alert')).toHaveTextContent('Only images are accepted.')
    })
    And('no preview is shown', () => {
      expect(screen.queryByRole('img')).not.toBeInTheDocument()
    })
  })
})
